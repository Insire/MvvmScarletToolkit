using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace MvvmScarletToolkit.ImageLoading.Caches
{
    public sealed class ImageMemoryCache<TImage> : IImageMemoryCache<TImage> where TImage : class
    {
        private const string Prefix = "ImageMemoryCache";

        private readonly ILogger<ImageMemoryCache<TImage>> _logger;
        private readonly IMemoryCache _memoryCache;
        private readonly ImageMemoryCacheOptions _options;

        public ImageMemoryCache(
            ILogger<ImageMemoryCache<TImage>> logger,
            IMemoryCache memoryCache,
            ImageMemoryCacheOptions options)
        {
            _logger = logger;
            _memoryCache = memoryCache;
            _options = options;
        }

        /// <inheritdoc />
        public Task<TImage?> GetImageAsync(Uri uri, ImageSize requestedSize, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested || !_options.IsEnabled)
            {
                return Task.FromResult<TImage?>(null);
            }

            var key = CreateKey(uri, requestedSize);
            if (_memoryCache.TryGetValue(key, out TImage? cachedImage))
            {
                _logger.LogDebug("Image with {Key} was found in memory", key);

                return Task.FromResult(cachedImage);
            }

            _logger.LogDebug("Image with {Key} could not be found in memory", key);

            return Task.FromResult<TImage?>(null);
        }

        /// <inheritdoc />
        public Task CacheImageAsync(TImage image, Uri uri, ImageSize requestedSize, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested || !_options.IsEnabled)
            {
                return Task.CompletedTask;
            }

            var key = CreateKey(uri, requestedSize);

            using (var cacheEntry = _memoryCache.CreateEntry(key))
            {
                cacheEntry.AbsoluteExpiration = DateTimeOffset.UtcNow.AddSeconds(_options.AbsoluteExpirationOffsetInSeconds);
                cacheEntry.Value = image;

                if (_logger.IsEnabled(LogLevel.Trace))
                {
                    cacheEntry.RegisterPostEvictionCallback(OnEviction);
                }
            }

            _logger.LogDebug("Image with {Key} has been cached in memory", key);

            return Task.CompletedTask;
        }

        private void OnEviction(object key, object? value, EvictionReason reason, object? state)
        {
            _logger.LogTrace("Image with {Key} was evicted for reason: '{Reason}'", key, reason);
        }

        private static string CreateKey(Uri uri, ImageSize requestedImageSize)
        {
            return $"{Prefix}_'{uri.OriginalString}'_w{requestedImageSize.Width}_h{requestedImageSize.Height}";
        }
    }
}
