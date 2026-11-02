using Microsoft.Extensions.Logging;
using Microsoft.IO;
using System.Buffers;

namespace MvvmScarletToolkit.ImageLoading
{
    public class ImageDataProvider : IImageDataProvider
    {
        private readonly ILogger<ImageDataProvider> _logger;
        private readonly RecyclableMemoryStreamManager _recyclableMemoryStreamManager;
        private readonly HttpClient _httpClient;
        private readonly ImageDataProviderOptions _options;
        private readonly long _maxSize;

        public ImageDataProvider(
            ILogger<ImageDataProvider> logger,
            RecyclableMemoryStreamManager recyclableMemoryStreamManager,
            HttpClient httpClient,
            ImageDataProviderOptions? options = null)
        {
            _logger = logger;
            _recyclableMemoryStreamManager = recyclableMemoryStreamManager;
            _httpClient = httpClient;
            _options = options ?? new ImageDataProviderOptions();
            _maxSize = _options.MaxDownloadSizeInBytes > 0 ? _options.MaxDownloadSizeInBytes : long.MaxValue;
        }

        /// <inheritdoc />
        public async Task<Stream> GetStreamAsync(Uri uri, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Stream.Null;
            }

            try
            {
                if (IsWebResource(uri))
                {
                    return await DownloadAsync(uri, cancellationToken).ConfigureAwait(false);
                }

                var source = uri.IsAbsoluteUri && !uri.IsFile
                    ? await GetImageStreamFromChildImplementation(uri, cancellationToken).ConfigureAwait(false)
                    : OpenFile(uri);

                if (source is null || source == Stream.Null)
                {
                    _logger.LogWarning("{Type} could not provide a stream for '{Url}'", GetType().Name, uri);

                    return Stream.Null;
                }

                await using (source)
                {
                    if (!source.CanRead || (source.CanSeek && source.Length == 0))
                    {
                        return Stream.Null;
                    }

                    if (source.CanSeek && source.Length > _maxSize)
                    {
                        _logger.LogWarning("Resource '{Url}' exceeds the size limit of {Limit} bytes", uri, _maxSize);

                        return Stream.Null;
                    }

                    return await CopyAsync(source, uri, cancellationToken, cancellationToken).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("Fetching stream from '{Url}' timed out", uri);

                return Stream.Null;
            }
            catch (OperationCanceledException)
            {
                return Stream.Null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fetching stream from '{Url}' failed", uri);

                return Stream.Null;
            }
        }

        protected virtual Task<Stream> GetImageStreamFromChildImplementation(Uri uri, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Stream.Null);
        }

        private static bool IsWebResource(Uri uri)
        {
            return uri.IsAbsoluteUri
                && (uri.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase) || uri.Scheme.Equals(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase));
        }

        private static FileStream OpenFile(Uri uri)
        {
            var path = uri.IsAbsoluteUri ? uri.LocalPath : uri.OriginalString;

            return new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete, 4096, FileOptions.Asynchronous | FileOptions.SequentialScan);
        }

        private async Task<Stream> DownloadAsync(Uri uri, CancellationToken cancellationToken)
        {
            using var linkedSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            if (_options.RequestTimeout > TimeSpan.Zero)
            {
                linkedSource.CancelAfter(_options.RequestTimeout);
            }

            var token = linkedSource.Token;

            _logger.LogTrace("Fetching stream from {Url}", uri);

            var responseTask = _httpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, token);
            if (!await CompletesAsync(responseTask, token).ConfigureAwait(false))
            {
                return Cancelled(uri, cancellationToken);
            }

            using var response = await responseTask.ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                // expected for e.g. removed images, not worth an exception
                _logger.LogWarning("Fetching stream from '{Url}' failed with status {StatusCode}", uri, (int)response.StatusCode);

                return Stream.Null;
            }

            if (response.Content.Headers.ContentLength > _maxSize)
            {
                _logger.LogWarning("Resource '{Url}' exceeds the size limit of {Limit} bytes", uri, _maxSize);

                return Stream.Null;
            }

            var sourceTask = response.Content.ReadAsStreamAsync(token);
            if (!await CompletesAsync(sourceTask, token).ConfigureAwait(false))
            {
                return Cancelled(uri, cancellationToken);
            }

            await using var source = await sourceTask.ConfigureAwait(false);

            return await CopyAsync(source, uri, cancellationToken, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Waits for <paramref name="task"/> without throwing when it got cancelled. Cancelling downloads is routine,
        /// e.g. for images scrolled out of a virtualized list, and every exception rethrown on the way up is expensive with a debugger attached.
        /// </summary>
        /// <returns><see langword="false"/> when <paramref name="task"/> got cancelled.</returns>
        private static async Task<bool> CompletesAsync(Task task, CancellationToken token)
        {
            await Task.WhenAny(task).ConfigureAwait(false);

            if (task.IsCanceled || (task.IsFaulted && token.IsCancellationRequested))
            {
                return false;
            }

            // surfaces real errors
            await task.ConfigureAwait(false);

            return true;
        }

        private Stream Cancelled(Uri uri, CancellationToken callerToken)
        {
            if (!callerToken.IsCancellationRequested)
            {
                _logger.LogWarning("Fetching stream from '{Url}' timed out", uri);
            }

            return Stream.Null;
        }

        private async Task<Stream> CopyAsync(Stream source, Uri uri, CancellationToken callerToken, CancellationToken cancellationToken)
        {
            var result = _recyclableMemoryStreamManager.GetStream();
            var success = false;
            var buffer = ArrayPool<byte>.Shared.Rent(81920);

            try
            {
                while (true)
                {
                    var pendingRead = source.ReadAsync(buffer.AsMemory(), cancellationToken);
                    int read;
                    if (pendingRead.IsCompletedSuccessfully)
                    {
                        read = pendingRead.Result;
                    }
                    else
                    {
                        var readTask = pendingRead.AsTask();
                        if (!await CompletesAsync(readTask, cancellationToken).ConfigureAwait(false))
                        {
                            return Cancelled(uri, callerToken);
                        }

                        read = await readTask.ConfigureAwait(false);
                    }

                    if (read == 0)
                    {
                        break;
                    }

                    if (result.Length + read > _maxSize)
                    {
                        _logger.LogWarning("Resource '{Url}' exceeds the size limit of {Limit} bytes", uri, _maxSize);

                        return Stream.Null;
                    }

                    result.Write(buffer, 0, read);
                }

                if (result.Length == 0)
                {
                    return Stream.Null;
                }

                result.Seek(0, SeekOrigin.Begin);
                success = true;

                return result;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);

                if (!success)
                {
                    await result.DisposeAsync().ConfigureAwait(false);
                }
            }
        }
    }
}
