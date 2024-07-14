using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using MvvmScarletToolkit.Abstractions.ImageLoading;
using System.Text;

namespace MvvmScarletToolkit.ImageLoading
{
    public sealed class ImageLoader<TImage> : IAsyncImageLoader<TImage>
        where TImage : class
    {
        private readonly ILogger<ImageLoader<TImage>> _logger;
        private readonly IImageFactory<TImage> _imageFactory;
        private readonly IImageDataProvider _imageDataProvider;
        private readonly IDiskCachedImageDataProvider _diskCachedImageDataProvider;
        private readonly IMemoryCachedImageDataProvider _memoryCachedImageDataProvider;
        private readonly MemoryCacheImageProvider<TImage> _memoryCacheImageProvider;
        private readonly RecyclableMemoryStreamManager _recyclableMemoryStreamManager;
        private readonly IMemoryCache _memoryCache;
        private readonly ImageLoaderOptions _options;
        private readonly string _prefix;

        public ImageLoader(
            ILogger<ImageLoader<TImage>> logger,
            IImageFactory<TImage> imageFactory,
            IImageDataProvider imageDataProvider,
            IDiskCachedImageDataProvider diskCachedImageDataProvider,
            IMemoryCachedImageDataProvider memoryCachedImageDataProvider,
            MemoryCacheImageProvider<TImage> memoryCacheImageProvider,
            IMemoryCache memoryCache,
            RecyclableMemoryStreamManager recyclableMemoryStreamManager,
            ImageLoaderOptions options)
        {
            _logger = logger;
            _imageFactory = imageFactory;
            _imageDataProvider = imageDataProvider;
            _diskCachedImageDataProvider = diskCachedImageDataProvider;
            _memoryCachedImageDataProvider = memoryCachedImageDataProvider;
            _memoryCacheImageProvider = memoryCacheImageProvider;
            _memoryCache = memoryCache;
            _recyclableMemoryStreamManager = recyclableMemoryStreamManager;
            _options = options;

            var bytes = Encoding.UTF8.GetBytes(GetType().Name);
            using var resultStream = _recyclableMemoryStreamManager.GetStream(null, bytes.Length);
            resultStream.Write(bytes, 0, bytes.Length);
            resultStream.Seek(0, SeekOrigin.Begin);

            _prefix = resultStream.CalculateMd5();
        }

        /// <inheritdoc />
        public async Task<TImage?> ProvideImageAsync(Uri? uri, ImageSize? requestedSize, Action<bool> requestedImageLoadsSlowly, CancellationToken cancellationToken = default)
        {
            if (uri is null)
            {
                return null;
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return null;
            }

            _logger.LogDebug("Image with {@Size} requested", requestedSize);

            var imageSize = GetImageSize(requestedSize, _options);
            var key = CreateKey(uri, imageSize);

            if (_memoryCache.TryGetValue(key, out var cachedLock) && cachedLock is SemaphoreSlim currentLock)
            {
                return await ProvideImageWithLockAsync(uri, imageSize, requestedImageLoadsSlowly, currentLock, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                using (var cacheEntry = _memoryCache.CreateEntry(key))
                {
                    cacheEntry.Value = currentLock = new SemaphoreSlim(1, 1);
                }

                return await ProvideImageWithLockAsync(uri, imageSize, requestedImageLoadsSlowly, currentLock, cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task<TImage?> ProvideImageWithLockAsync(
            Uri uri,
            ImageSize requestedSize,
            Action<bool> requestedImageLoadsSlowly,
            SemaphoreSlim currentLock,
            CancellationToken cancellationToken)
        {
            try
            {
                await currentLock.WaitAsync(cancellationToken).ConfigureAwait(false);

                return await ProvideImageAsync(uri, requestedSize, requestedImageLoadsSlowly, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                currentLock.Release();
            }
        }

        private async Task<TImage?> ProvideImageAsync(Uri uri, ImageSize requestedSize, Action<bool> requestedImageLoadsSlowly, CancellationToken cancellationToken)
        {
            var image = await _memoryCacheImageProvider.GetImageAsync(uri, requestedSize, cancellationToken).ConfigureAwait(false);
            if (image is not null)
            {
                return image;
            }

            var stream = await _memoryCachedImageDataProvider.GetStreamAsync(uri, requestedSize, cancellationToken).ConfigureAwait(false);
            if (stream is not null)
            {
                image = _imageFactory.From(stream, requestedSize);

                await _memoryCacheImageProvider.CacheImageAsync(image, uri, requestedSize, cancellationToken).ConfigureAwait(false);
            }

            stream = await _diskCachedImageDataProvider.GetStreamAsync(uri, requestedSize, cancellationToken).ConfigureAwait(false);
            if (stream is not null)
            {
                image = _imageFactory.From(stream, requestedSize);

                await _memoryCacheImageProvider.CacheImageAsync(image, uri, requestedSize, cancellationToken).ConfigureAwait(false);
                await _memoryCachedImageDataProvider.CacheStreamAsync(stream, uri, requestedSize, cancellationToken).ConfigureAwait(false);

                return image;
            }

            requestedImageLoadsSlowly(true);

            stream = await _imageDataProvider.GetStreamAsync(uri, cancellationToken).ConfigureAwait(false);
            if (stream is not null)
            {
                image = _imageFactory.From(stream, requestedSize);

                await _memoryCacheImageProvider.CacheImageAsync(image, uri, requestedSize, cancellationToken).ConfigureAwait(false);
                await _memoryCachedImageDataProvider.CacheStreamAsync(stream, uri, requestedSize, cancellationToken).ConfigureAwait(false);
                await _diskCachedImageDataProvider.CacheStreamAsync(stream, uri, requestedSize, cancellationToken).ConfigureAwait(false);

                return image;
            }

            return null;
        }

        private string CreateKey(Uri uri, ImageSize requestedImageSize)
        {
            return $"{_prefix}_'{uri.OriginalString}'_w{requestedImageSize.Width}_h{requestedImageSize.Height}";
        }

        private static ImageSize GetImageSize(ImageSize? requestedImageSize, ImageLoaderOptions options)
        {
            if (requestedImageSize is null)
            {
                return new ImageSize(options.DefaultWidth, options.DefaultHeight);
            }

            return new ImageSize(requestedImageSize.Value.Width, requestedImageSize.Value.Height);
        }
    }
}
