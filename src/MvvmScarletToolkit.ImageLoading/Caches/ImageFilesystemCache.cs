using Microsoft.Extensions.Logging;

namespace MvvmScarletToolkit.ImageLoading.Caches
{
    public sealed class ImageFilesystemCache<TImage> : IImageFilesystemCache<TImage>
        where TImage : class
    {
        private const string Scope = "image";

        private readonly ILogger<ImageFilesystemCache<TImage>> _logger;
        private readonly IImageFactory<TImage> _imageFactory;
        private readonly ImageFilesystemCacheOptions _options;

        public ImageFilesystemCache(
            ILogger<ImageFilesystemCache<TImage>> logger,
            IImageFactory<TImage> imageFactory,
            ImageFilesystemCacheOptions options)
        {
            _logger = logger;
            _imageFactory = imageFactory;
            _options = options;

            CacheFile.Initialize(logger, options.CacheDirectoryPath, options.CreateFolder, options.ClearCacheDirectoryOnInit, options.MaxFileAgeInDays);
        }

        /// <inheritdoc />
        public async Task<TImage?> GetImageAsync(Uri uri, ImageSize requestedSize, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested || !_options.IsEnabled)
            {
                return null;
            }

            var key = CacheFile.CreateName(Scope, uri, requestedSize);
            var path = Path.Combine(_options.CacheDirectoryPath, key);

            FileStream? stream;
            try
            {
                stream = CacheFile.TryOpenRead(path);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                _logger.LogWarning(ex, "Opening file {FilePath} failed unexpectedly", path);

                return null;
            }

            if (stream is null)
            {
                _logger.LogDebug("Image with {Key} could not be found on disk at {Path}", key, path);

                return null;
            }

            try
            {
                _logger.LogDebug("Image with {Key} was found on disk at {Path}", key, path);

                await using (stream)
                {
                    // decoding is CPU bound, keep it off the caller's thread. Not cancellable on purpose,
                    // it is short and the caller caches the result, so the work is not lost
                    return await Task.Run(() => _imageFactory.FromAsync(stream, requestedSize, CancellationToken.None)).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                // the caller no longer needs the image, not an error
                return null;
            }
            catch (Exception ex)
            {
                // a corrupt entry would otherwise be hit forever
                _logger.LogWarning(ex, "Reading image from {FilePath} failed, removing cache entry", path);
                CacheFile.TryDelete(path);

                return null;
            }
        }

        /// <inheritdoc />
        public async Task CacheImageAsync(TImage image, Uri uri, ImageSize requestedSize, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested || !_options.IsEnabled)
            {
                return;
            }

            var key = CacheFile.CreateName(Scope, uri, requestedSize);
            var path = Path.Combine(_options.CacheDirectoryPath, key);

            if (File.Exists(path))
            {
                return;
            }

            try
            {
                if (await CacheFile.WriteAtomicAsync(path, stream => Task.Run(() => _imageFactory.ToAsync(stream, image, cancellationToken), cancellationToken), cancellationToken).ConfigureAwait(false))
                {
                    _logger.LogDebug("Image with {Key} has been cached on disk at {Path}", key, path);
                }
                else
                {
                    _logger.LogDebug("Image with {Key} is in use on disk at {Path}, keeping the existing file", key, path);
                }
            }
            catch (OperationCanceledException)
            {
                // nothing got written, the next load tries again
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Writing file {FilePath} failed unexpectedly", path);
            }
        }
    }
}
