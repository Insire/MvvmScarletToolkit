using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using MvvmScarletToolkit.Abstractions.ImageLoading;
using System.Text;

namespace MvvmScarletToolkit.ImageLoading
{
    /// <summary>
    /// Provides memory and disk cached way to asynchronously load images for <see cref="ImageLoader"/>
    /// Can be used as base class if you want to create custom caching mechanism
    /// </summary>
    public class DiskCachedWebImageLoader<T> : RamCachedWebImageLoader<T>
        where T : class
    {
        private readonly string _cacheFolder;

        public DiskCachedWebImageLoader(
            ILogger<DiskCachedWebImageLoader<T>> log,
            MemoryCache memoryCache,
            RecyclableMemoryStreamManager streamManager,
            IImageFactory<T> imageFactory,
            DiskCachedWebImageLoaderOptions options)
            : base(log, memoryCache, streamManager, imageFactory)
        {
            _cacheFolder = options.CacheFolder;

            if (options.CreateFolder)
            {
                Directory.CreateDirectory(options.CacheFolder);
            }
        }

        public DiskCachedWebImageLoader(
            ILogger<DiskCachedWebImageLoader<T>> log,
            MemoryCache memoryCache,
            HttpClient httpClient,
            RecyclableMemoryStreamManager streamManager,
            IImageFactory<T> imageFactory,
            DiskCachedWebImageLoaderOptions options)
            : base(log, httpClient, memoryCache, streamManager, imageFactory, options)
        {
            _cacheFolder = options.CacheFolder;

            if (options.CreateFolder)
            {
                Directory.CreateDirectory(options.CacheFolder);
            }
        }

        /// <inheritdoc />
        protected override async Task<T?> LoadFromGlobalCache(Uri? url, ImageSize requestedSize, Action<bool> requestedImageLoadsSlowly)
        {
            if (url is null)
            {
                return null;
            }

            var md5 = await CreateMD5(url, requestedSize);
            var path = Path.Combine(_cacheFolder, md5);
            if (!File.Exists(path))
            {
                Log.LogTrace("[CACHEMISS] for {Url}", url);
                return null;
            }

            requestedImageLoadsSlowly(true);
            return await Task.Run(() => ImageFactory.From(path, requestedSize));
        }

        protected override async Task SaveToGlobalCache(Uri? url, ImageSize imageSize, Stream imageBytes)
        {
            if (url is null)
            {
                return;
            }

            var md5 = await CreateMD5(url, imageSize);
            var path = Path.Combine(_cacheFolder, md5);

            imageBytes.Seek(0, SeekOrigin.Begin);

            await using var fileStream = File.OpenWrite(path);
            await imageBytes.CopyToAsync(fileStream);
        }

        protected async Task<string> CreateMD5(Uri? input, ImageSize imageSize)
        {
            if (input is null)
            {
                return string.Empty;
            }

            await using var resultStream = StreamManager.GetStream();
            var bytes = Encoding.ASCII.GetBytes(input.OriginalString + '_' + imageSize.Width + '_' + imageSize.Height);

            resultStream.Write(bytes, 0, bytes.Length);
            resultStream.Seek(0, SeekOrigin.Begin);

            return await resultStream.CalculateMd5().ConfigureAwait(false);
        }
    }
}
