using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using MvvmScarletToolkit.Abstractions.ImageLoading;
using System.Text;

namespace MvvmScarletToolkit.ImageLoading
{
    public sealed class MemoryCacheImageDataProvider : IMemoryCachedImageDataProvider
    {
        private readonly IMemoryCache _memoryCache;
        private readonly MemoryCacheImageDataProviderOptions _options;
        private readonly ILogger<MemoryCacheImageDataProvider> _logger;
        private readonly RecyclableMemoryStreamManager _recyclableMemoryStreamManager;
        private readonly string _prefix;

        public MemoryCacheImageDataProvider(
            ILogger<MemoryCacheImageDataProvider> logger,
            IMemoryCache memoryCache,
            RecyclableMemoryStreamManager recyclableMemoryStreamManager,
            MemoryCacheImageDataProviderOptions options)
        {
            _memoryCache = memoryCache;
            _recyclableMemoryStreamManager = recyclableMemoryStreamManager;
            _options = options;
            _logger = logger;

            var bytes = Encoding.UTF8.GetBytes(nameof(MemoryCacheImageDataProvider));
            using var resultStream = _recyclableMemoryStreamManager.GetStream(null, bytes.Length);
            resultStream.Write(bytes, 0, bytes.Length);
            resultStream.Seek(0, SeekOrigin.Begin);

            _prefix = resultStream.CalculateMd5();
        }

        /// <inheritdoc />
        public Task<Stream?> GetStreamAsync(Uri uri, ImageSize requestedSize, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromResult<Stream?>(null);
            }

            if (_options.IsEnabled == false)
            {
                return Task.FromResult<Stream?>(null);
            }

            var key = CreateKey(uri, requestedSize);
            if (_memoryCache.TryGetValue(key, out Stream? cachedImage))
            {
                _logger.LogDebug("Resource with {Key} was found in memory", key);

                cachedImage!.Seek(0, SeekOrigin.Begin);

                return Task.FromResult<Stream?>(cachedImage);
            }

            _logger.LogDebug("Resource with {Key} could not be found in memory", key);

            return Task.FromResult<Stream?>(null);
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
