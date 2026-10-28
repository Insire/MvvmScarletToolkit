using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace MvvmScarletToolkit.ImageLoading.Caches.Data
{
    public sealed class ImageDataMemoryCache : IImageDataMemoryCache
    {
        private const string Prefix = nameof(ImageDataMemoryCache);

        private readonly IMemoryCache _memoryCache;
        private readonly ImageDataMemoryCacheOptions _options;
        private readonly ILogger<ImageDataMemoryCache> _logger;

        public ImageDataMemoryCache(
            ILogger<ImageDataMemoryCache> logger,
            IMemoryCache memoryCache,
            ImageDataMemoryCacheOptions options)
        {
            _memoryCache = memoryCache;
            _options = options;
            _logger = logger;
        }

        /// <inheritdoc />
        /// <remarks>Every call returns a new, independent, read-only stream that the caller owns.</remarks>
        public Task<Stream> GetStreamAsync(Uri uri, ImageSize requestedSize, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested || !_options.IsEnabled)
            {
                return Task.FromResult(Stream.Null);
            }

            var key = CreateKey(uri, requestedSize);
            if (_memoryCache.TryGetValue(key, out byte[]? cachedBytes) && cachedBytes is not null)
            {
                _logger.LogDebug("Resource with {Key} was found in memory", key);

                return Task.FromResult<Stream>(new MemoryStream(cachedBytes, writable: false));
            }

            _logger.LogDebug("Resource with {Key} could not be found in memory", key);

            return Task.FromResult(Stream.Null);
        }

        /// <inheritdoc />
        /// <remarks>The stream is copied; the caller keeps ownership of it.</remarks>
        public async Task CacheStreamAsync(Stream imageDataStream, Uri uri, ImageSize requestedSize, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested || !_options.IsEnabled)
            {
                return;
            }

            var key = CreateKey(uri, requestedSize);
            var bytes = await ReadAllBytesAsync(imageDataStream, cancellationToken).ConfigureAwait(false);
            if (bytes.Length == 0)
            {
                return;
            }

            using (var cacheEntry = _memoryCache.CreateEntry(key))
            {
                cacheEntry.AbsoluteExpiration = DateTimeOffset.UtcNow.AddSeconds(_options.AbsoluteExpirationOffsetInSeconds);
                cacheEntry.Size = bytes.Length;
                cacheEntry.Value = bytes;

                if (_logger.IsEnabled(LogLevel.Trace))
                {
                    cacheEntry.RegisterPostEvictionCallback(OnEviction);
                }
            }

            _logger.LogDebug("Resource with {Key} has been cached in memory", key);
        }

        private static async Task<byte[]> ReadAllBytesAsync(Stream stream, CancellationToken cancellationToken)
        {
            if (stream is MemoryStream memoryStream)
            {
                return memoryStream.ToArray();
            }

            if (stream.CanSeek)
            {
                stream.Seek(0, SeekOrigin.Begin);
            }

            using var copy = new MemoryStream(stream.CanSeek ? (int)Math.Min(stream.Length, int.MaxValue) : 0);
            await stream.CopyToAsync(copy, cancellationToken).ConfigureAwait(false);

            return copy.ToArray();
        }

        private void OnEviction(object key, object? value, EvictionReason reason, object? state)
        {
            _logger.LogTrace("Resource with {Key} was evicted for reason: '{Reason}'", key, reason);
        }

        private static string CreateKey(Uri uri, ImageSize requestedImageSize)
        {
            return $"{Prefix}_'{uri.OriginalString}'_w{requestedImageSize.Width}_h{requestedImageSize.Height}";
        }
    }
}
