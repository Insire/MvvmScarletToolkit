using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace MvvmScarletToolkit.ImageLoading
{
    public sealed class ImageService<TImage> : IImageService<TImage>
        where TImage : class
    {
        private const string FailurePrefix = "ImageService_failed_";

        private sealed class KeyedLock
        {
            public SemaphoreSlim Semaphore { get; } = new SemaphoreSlim(1, 1);

            public int References { get; set; }
        }

        private readonly ILogger<ImageService<TImage>> _logger;
        private readonly IImageFactory<TImage> _imageFactory;
        private readonly IImageDataProvider _imageDataProvider;
        private readonly IImageDataFileystemCache _diskCachedImageDataProvider;
        private readonly IImageFilesystemCache<TImage> _diskCachedImageProvider;
        private readonly IImageDataMemoryCache _memoryCachedImageDataProvider;
        private readonly IImageMemoryCache<TImage> _memoryCacheImageProvider;
        private readonly IMemoryCache _memoryCache;
        private readonly ImageServiceOptions _options;
        private readonly Dictionary<string, KeyedLock> _locks;
        private readonly SemaphoreSlim _diskCacheWrites;
        private readonly HashSet<Task> _pendingDiskCacheWrites;

        public ImageService(
            ILogger<ImageService<TImage>> logger,
            IImageFactory<TImage> imageFactory,
            IImageDataProvider imageDataProvider,
            IImageDataFileystemCache diskCachedImageDataProvider,
            IImageFilesystemCache<TImage> diskCachedImageProvider,
            IImageDataMemoryCache memoryCachedImageDataProvider,
            IImageMemoryCache<TImage> memoryCacheImageProvider,
            IMemoryCache memoryCache,
            ImageServiceOptions options)
        {
            _logger = logger;
            _imageFactory = imageFactory;
            _imageDataProvider = imageDataProvider;
            _diskCachedImageDataProvider = diskCachedImageDataProvider;
            _diskCachedImageProvider = diskCachedImageProvider;
            _memoryCachedImageDataProvider = memoryCachedImageDataProvider;
            _memoryCacheImageProvider = memoryCacheImageProvider;
            _memoryCache = memoryCache;
            _options = options;
            _locks = new Dictionary<string, KeyedLock>();
            _diskCacheWrites = new SemaphoreSlim(Math.Max(1, options.MaxConcurrentDiskCacheWrites));
            _pendingDiskCacheWrites = new HashSet<Task>();
        }

        /// <summary>
        /// Waits until the disk cache writes queued so far have completed. Images are handed out before they are written
        /// to the disk caches, call this e.g. before the application exits to keep everything that was loaded.
        /// </summary>
        public async Task FlushAsync(CancellationToken cancellationToken = default)
        {
            while (true)
            {
                Task[] pending;
                lock (_pendingDiskCacheWrites)
                {
                    pending = [.. _pendingDiskCacheWrites];
                }

                if (pending.Length == 0)
                {
                    return;
                }

                // writes queued in the meantime are picked up by the next round
                await Task.WhenAll(pending).WaitAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        /// <inheritdoc />
        /// <returns>
        /// The image, or <see langword="null"/> when it could not be loaded or <paramref name="cancellationToken"/> was cancelled.
        /// </returns>
        /// <remarks>
        /// Cancellation is routine, e.g. for every image scrolled out of a virtualized list, so it is reported with <see langword="null"/>
        /// instead of exceptions, which are expensive, especially with a debugger attached.
        /// </remarks>
        public async Task<TImage?> ProvideImageAsync(Uri? uri, ImageSize? requestedSize, Func<bool, Task> requestedImageLoadsSlowly, CancellationToken cancellationToken = default)
        {
            if (uri is null || cancellationToken.IsCancellationRequested)
            {
                return null;
            }

            _logger.LogDebug("Image with {@Size} requested", requestedSize);

            var imageSize = GetImageSize(requestedSize, _options);
            var key = CreateKey(uri, imageSize);
            var failureKey = FailurePrefix + key;

            if (_memoryCache.TryGetValue(failureKey, out _))
            {
                _logger.LogDebug("Loading image from {Uri} was skipped, because it failed recently", uri.OriginalString);

                return null;
            }

            var keyedLock = AcquireLock(key);
            try
            {
                // not inside the inner try block, releasing a semaphore that was never acquired corrupts its count
                if (!await TryEnterAsync(keyedLock.Semaphore, cancellationToken).ConfigureAwait(false))
                {
                    return null;
                }

                try
                {
                    // a previous holder of the lock might have failed while this call was waiting
                    if (_memoryCache.TryGetValue(failureKey, out _))
                    {
                        return null;
                    }

                    var image = await LoadAsync(uri, imageSize, requestedImageLoadsSlowly, cancellationToken).ConfigureAwait(false);

                    // a cancelled load is not a failed load
                    if (image is null && !cancellationToken.IsCancellationRequested)
                    {
                        RememberFailure(failureKey);
                    }

                    return image;
                }
                finally
                {
                    keyedLock.Semaphore.Release();
                }
            }
            finally
            {
                ReleaseLock(key, keyedLock);
            }
        }

        /// <remarks>
        /// Every tier returns <see langword="null"/> when <paramref name="cancellationToken"/> gets cancelled, no exceptions.
        /// Once an image has been obtained it is cached regardless of cancellation, so the work is not lost and the next request is fast.
        /// The memory caches are filled before the image is returned, the disk caches afterwards in the background,
        /// so encoding and file IO never delay showing an image.
        /// </remarks>
        private async Task<TImage?> LoadAsync(Uri uri, ImageSize requestedSize, Func<bool, Task> requestedImageLoadsSlowly, CancellationToken cancellationToken)
        {
            // memory image
            var image = await _memoryCacheImageProvider.GetImageAsync(uri, requestedSize, cancellationToken).ConfigureAwait(false);
            if (image is not null)
            {
                return image;
            }

            // memory image data
            await using (var memoryStream = await _memoryCachedImageDataProvider.GetStreamAsync(uri, requestedSize, cancellationToken).ConfigureAwait(false))
            {
                image = await TryCreateImageAsync(memoryStream, uri, requestedSize, LogLevel.Warning).ConfigureAwait(false);
                if (image is not null)
                {
                    await _memoryCacheImageProvider.CacheImageAsync(image, uri, requestedSize, CancellationToken.None).ConfigureAwait(false);

                    var encoded = image;
                    QueueDiskCacheWrite(() => _diskCachedImageProvider.CacheImageAsync(encoded, uri, requestedSize, CancellationToken.None));

                    return image;
                }
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return null;
            }

            await NotifyLoadsSlowlyAsync(requestedImageLoadsSlowly).ConfigureAwait(false);

            // filesystem image
            image = await _diskCachedImageProvider.GetImageAsync(uri, requestedSize, cancellationToken).ConfigureAwait(false);
            if (image is not null)
            {
                await _memoryCacheImageProvider.CacheImageAsync(image, uri, requestedSize, CancellationToken.None).ConfigureAwait(false);

                return image;
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return null;
            }

            // filesystem data
            await using (var fileStream = await _diskCachedImageDataProvider.GetStreamAsync(uri, requestedSize, cancellationToken).ConfigureAwait(false))
            {
                image = await TryCreateImageAsync(fileStream, uri, requestedSize, LogLevel.Warning).ConfigureAwait(false);
                if (image is not null)
                {
                    await _memoryCacheImageProvider.CacheImageAsync(image, uri, requestedSize, CancellationToken.None).ConfigureAwait(false);

                    // copies the data, the file stream is not needed afterwards
                    await _memoryCachedImageDataProvider.CacheStreamAsync(fileStream, uri, requestedSize, CancellationToken.None).ConfigureAwait(false);

                    var encoded = image;
                    QueueDiskCacheWrite(() => _diskCachedImageProvider.CacheImageAsync(encoded, uri, requestedSize, CancellationToken.None));

                    return image;
                }
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return null;
            }

            // remote (web) or filesystem data
            var remoteStream = await _imageDataProvider.GetStreamAsync(uri, cancellationToken).ConfigureAwait(false);
            var isHandedOver = false;
            try
            {
                image = await TryCreateImageAsync(remoteStream, uri, requestedSize, LogLevel.Error).ConfigureAwait(false);
                if (image is null)
                {
                    return null;
                }

                await _memoryCacheImageProvider.CacheImageAsync(image, uri, requestedSize, CancellationToken.None).ConfigureAwait(false);

                // copies the data, so it does not depend on the stream's lifetime
                await _memoryCachedImageDataProvider.CacheStreamAsync(remoteStream, uri, requestedSize, CancellationToken.None).ConfigureAwait(false);

                // from here on the background write owns the stream and disposes it, this method must not touch it anymore
                var encoded = image;
                QueueDiskCacheWrite(async () =>
                {
                    await using (remoteStream)
                    {
                        await _diskCachedImageDataProvider.CacheStreamAsync(remoteStream, uri, requestedSize, CancellationToken.None).ConfigureAwait(false);
                    }

                    await _diskCachedImageProvider.CacheImageAsync(encoded, uri, requestedSize, CancellationToken.None).ConfigureAwait(false);
                });
                isHandedOver = true;

                return image;
            }
            finally
            {
                if (!isHandedOver)
                {
                    await remoteStream.DisposeAsync().ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Runs <paramref name="write"/> in the background, see <see cref="ImageServiceOptions.MaxConcurrentDiskCacheWrites"/> and <see cref="FlushAsync"/>.
        /// </summary>
        /// <remarks>
        /// <paramref name="write"/> is guaranteed to run, so it can take ownership of resources like streams and dispose them.
        /// </remarks>
        private void QueueDiskCacheWrite(Func<Task> write)
        {
            var task = Task.Run(async () =>
            {
                await _diskCacheWrites.WaitAsync().ConfigureAwait(false);
                try
                {
                    await write().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    // nobody observes this task, the disk caches log their own failures already
                    _logger.LogError(ex, "Writing to the disk cache failed unexpectedly");
                }
                finally
                {
                    _diskCacheWrites.Release();
                }
            });

            lock (_pendingDiskCacheWrites)
            {
                _pendingDiskCacheWrites.Add(task);
            }

            // registered after adding, so the removal can never run before the add
            task.ContinueWith(
                static (completed, state) =>
                {
                    var pending = (HashSet<Task>)state!;
                    lock (pending)
                    {
                        pending.Remove(completed);
                    }
                },
                _pendingDiskCacheWrites,
                CancellationToken.None,
                TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Default);
        }

        private async Task<TImage?> TryCreateImageAsync(Stream stream, Uri uri, ImageSize requestedSize, LogLevel failureLevel)
        {
            if (stream == Stream.Null)
            {
                return null;
            }

            try
            {
                // decoding is CPU bound, never on the caller's (UI) thread. The in-memory tiers complete synchronously,
                // so ConfigureAwait(false) alone would not leave the caller's thread before getting here.
                // Not cancellable on purpose, the data is already here and decoding is short, the result gets cached either way
                return await Task.Run(() => _imageFactory.FromAsync(stream, requestedSize, CancellationToken.None)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.Log(failureLevel, ex, "Loading image from {Uri} failed unexpectedly", uri.OriginalString);

                return null;
            }
        }

        private async Task NotifyLoadsSlowlyAsync(Func<bool, Task> requestedImageLoadsSlowly)
        {
            try
            {
                await requestedImageLoadsSlowly(true).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // a misbehaving callback must not fail the load
                _logger.LogWarning(ex, "Notifying about a slowly loading image failed");
            }
        }

        private void RememberFailure(string failureKey)
        {
            if (_options.FailedLoadRetryDelayInSeconds <= 0)
            {
                return;
            }

            using var cacheEntry = _memoryCache.CreateEntry(failureKey);
            cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(_options.FailedLoadRetryDelayInSeconds);
            cacheEntry.Size = 1;
            cacheEntry.Value = true;
        }

        /// <summary>
        /// Waits for <paramref name="semaphore"/> until <paramref name="cancellationToken"/> gets cancelled, without throwing.
        /// <see cref="SemaphoreSlim.WaitAsync(CancellationToken)"/> throws internally when cancelled while waiting.
        /// </summary>
        /// <returns><see langword="true"/> when the semaphore was entered and has to be released.</returns>
        private static async Task<bool> TryEnterAsync(SemaphoreSlim semaphore, CancellationToken cancellationToken)
        {
            var wait = semaphore.WaitAsync();
            if (wait.IsCompleted)
            {
                return true;
            }

            var cancelled = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            using (cancellationToken.UnsafeRegister(static state => ((TaskCompletionSource)state!).TrySetResult(), cancelled))
            {
                if (await Task.WhenAny(wait, cancelled.Task).ConfigureAwait(false) == wait)
                {
                    return true;
                }
            }

            // gave up, pass the semaphore on as soon as it would have been entered
            _ = wait.ContinueWith(static (_, state) => ((SemaphoreSlim)state!).Release(), semaphore, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);

            return false;
        }

        private KeyedLock AcquireLock(string key)
        {
            lock (_locks)
            {
                if (!_locks.TryGetValue(key, out var keyedLock))
                {
                    keyedLock = new KeyedLock();
                    _locks.Add(key, keyedLock);
                }

                keyedLock.References++;

                return keyedLock;
            }
        }

        private void ReleaseLock(string key, KeyedLock keyedLock)
        {
            lock (_locks)
            {
                if (--keyedLock.References == 0)
                {
                    _locks.Remove(key);
                }
            }
        }

        private static string CreateKey(Uri uri, ImageSize requestedImageSize)
        {
            return $"'{uri.OriginalString}'_w{requestedImageSize.Width}_h{requestedImageSize.Height}";
        }

        private static ImageSize GetImageSize(ImageSize? requestedImageSize, ImageServiceOptions options)
        {
            return requestedImageSize is null
                ? new ImageSize(options.DefaultWidth, options.DefaultHeight)
                : new ImageSize(requestedImageSize.Value.Width, requestedImageSize.Value.Height);
        }
    }
}
