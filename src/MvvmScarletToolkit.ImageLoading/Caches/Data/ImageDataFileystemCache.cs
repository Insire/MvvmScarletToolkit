using Microsoft.Extensions.Logging;

namespace MvvmScarletToolkit.ImageLoading.Caches.Data
{
    public sealed class ImageDataFileystemCache : IImageDataFileystemCache
    {
        private const string Scope = "data";

        private readonly ILogger<ImageDataFileystemCache> _logger;
        private readonly ImageDataFileystemCacheOptions _options;

        public ImageDataFileystemCache(
            ILogger<ImageDataFileystemCache> logger,
            ImageDataFileystemCacheOptions options)
        {
            _logger = logger;
            _options = options;

            CacheFile.Initialize(logger, options.CacheDirectoryPath, options.CreateFolder, options.ClearCacheDirectoryOnInit, options.MaxFileAgeInDays);
        }

        /// <inheritdoc />
        public Task<Stream> GetStreamAsync(Uri uri, ImageSize requestedSize, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested || !_options.IsEnabled)
            {
                return Task.FromResult(Stream.Null);
            }

            var key = CacheFile.CreateName(Scope, uri, requestedSize);
            var path = Path.Combine(_options.CacheDirectoryPath, key);

            try
            {
                var stream = CacheFile.TryOpenRead(path);
                if (stream is not null)
                {
                    _logger.LogDebug("Resource with {Key} was found on disk", key);

                    return Task.FromResult<Stream>(stream);
                }

                _logger.LogDebug("Resource with {Key} could not be found on disk", key);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                _logger.LogWarning(ex, "Opening file {FilePath} failed unexpectedly", path);
            }

            return Task.FromResult(Stream.Null);
        }

        /// <inheritdoc />
        public async Task CacheStreamAsync(Stream imageDataStream, Uri uri, ImageSize requestedSize, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested || !_options.IsEnabled)
            {
                return;
            }

            var key = CacheFile.CreateName(Scope, uri, requestedSize);
            var path = Path.Combine(_options.CacheDirectoryPath, key);

            try
            {
                if (imageDataStream.CanSeek)
                {
                    imageDataStream.Seek(0, SeekOrigin.Begin);
                }

                if (await CacheFile.WriteAtomicAsync(path, fileStream => imageDataStream.CopyToAsync(fileStream, cancellationToken), cancellationToken).ConfigureAwait(false))
                {
                    _logger.LogDebug("Resource with {Key} has been cached on disk", key);
                }
                else
                {
                    _logger.LogDebug("Resource with {Key} is in use on disk, keeping the existing file", key);
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
