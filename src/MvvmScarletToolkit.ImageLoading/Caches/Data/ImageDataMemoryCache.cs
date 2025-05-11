using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using System.Text;

namespace MvvmScarletToolkit.ImageLoading.Caches.Data
{
    public sealed class ImageDataMemoryCache : IImageDataMemoryCache
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ImageDataMemoryCacheOptions _options;
        private readonly ILogger<ImageDataMemoryCache> _logger;
        private readonly RecyclableMemoryStreamManager _recyclableMemoryStreamManager;
        private readonly string _prefix;

        public ImageDataMemoryCache(
            ILogger<ImageDataMemoryCache> logger,
            IMemoryCache memoryCache,
            RecyclableMemoryStreamManager recyclableMemoryStreamManager,
            ImageDataMemoryCacheOptions options)
        {
            _memoryCache = memoryCache;
            _recyclableMemoryStreamManager = recyclableMemoryStreamManager;
            _options = options;
            _logger = logger;

            var bytes = Encoding.UTF8.GetBytes(nameof(ImageDataMemoryCache));
            using var resultStream = _recyclableMemoryStreamManager.GetStream(null, bytes.Length);
            resultStream.Write(bytes, 0, bytes.Length);
            resultStream.Seek(0, SeekOrigin.Begin);

            _prefix = resultStream.CalculateMd5();
        }

        /// <inheritdoc />
        public Task<Stream> GetStreamAsync(Uri uri, ImageSize requestedSize, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromResult(Stream.Null);
            }

            if (_options.IsEnabled == false)
            {
                return Task.FromResult(Stream.Null);
            }

            var key = CreateKey(uri, requestedSize);
            if (_memoryCache.TryGetValue(key, out Stream? cachedImageStream))
            {
                if (cachedImageStream is Stream nonNullStream)
                {
                    _logger.LogDebug("Resource with {Key} was found in memory", key);
                    nonNullStream.Seek(0, SeekOrigin.Begin);

                    return Task.FromResult(nonNullStream);
                }
                else
                {
                    _logger.LogDebug("Resource with {Key} was found in memory, but it was null", key);

                    return Task.FromResult(Stream.Null);
                }
            }

            _logger.LogDebug("Resource with {Key} could not be found in memory", key);

            return Task.FromResult(Stream.Null);
        }

        /// <inheritdoc />
        public Task CacheStreamAsync(Stream imageDataStream, Uri uri, ImageSize requestedSize, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.CompletedTask;
            }

            if (_options.IsEnabled == false)
            {
                return Task.CompletedTask;
            }

            var key = CreateKey(uri, requestedSize);

            imageDataStream.Seek(0, SeekOrigin.Begin);

            using (var cacheEntry = _memoryCache.CreateEntry(key))
            {
                cacheEntry.AbsoluteExpiration = DateTimeOffset.UtcNow.AddSeconds(_options.AbsoluteExpirationOffsetInSeconds);
                cacheEntry.Value = imageDataStream;
                cacheEntry.RegisterPostEvictionCallback(OnEviction);
            }

            _logger.LogDebug("Resource with {Key} has been cached in memory", key);

            return Task.CompletedTask;
        }

        private void OnEviction(object key, object? value, EvictionReason reason, object? state)
        {
            _logger.LogTrace("Resource with {Key} was evicted for reason: '{Reason}'", key, reason);
        }

        private string CreateKey(Uri uri, ImageSize requestedImageSize)
        {
            return $"{_prefix}_'{uri.OriginalString}'_w{requestedImageSize.Width}_h{requestedImageSize.Height}";
        }
    }
}
