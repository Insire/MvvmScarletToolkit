using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using MvvmScarletToolkit.Abstractions.ImageLoading;

namespace MvvmScarletToolkit.ImageLoading
{
    /// <summary>
    /// Provides memory cached way to asynchronously load images for <see cref="ImageLoader"/>
    /// Can be used as base class if you want to create custom in memory caching
    /// </summary>
    public class RamCachedWebImageLoader<T> : BaseWebImageLoader<T>
        where T : class
    {
        public RamCachedWebImageLoader(ILogger<RamCachedWebImageLoader<T>> log, MemoryCache memoryCache, RecyclableMemoryStreamManager streamManager, IImageFactory<T> imageFactory)
            : base(log, memoryCache, streamManager, imageFactory)
        {
        }

        public RamCachedWebImageLoader(
            ILogger<RamCachedWebImageLoader<T>> log,
            HttpClient httpClient,
            MemoryCache memoryCache,
            RecyclableMemoryStreamManager streamManager,
            IImageFactory<T> imageFactory,
            BaseWebImageLoaderOptions options)
            : base(log, httpClient, memoryCache, streamManager, imageFactory, options)
        {
        }

        /// <inheritdoc />
        public override Task<T?> ProvideImageAsync(Uri? url, ImageSize requestedSize, Action<bool> requestedImageLoadsSlowly)
        {
            if (url is null)
            {
                return Task.FromResult<T?>(null);
            }

            return GetOrCreate(GetKey(url), () => LoadAsync(url, requestedSize, requestedImageLoadsSlowly), requestedImageLoadsSlowly);
        }

        private async Task<T?> GetOrCreate(string key, Func<Task<T?>> createImage, Action<bool> requestedImageLoadsSlowly)
        {
            var imageKey = CreateImageKey(key);
            if (!MemoryCache.TryGetValue(imageKey, out T? cachedImage))
            {
                var lockKey = CreateLockKey(key);
                while (true)
                {
                    if (MemoryCache.TryGetValue(lockKey, out var cachedLock) && cachedLock is SemaphoreSlim currentLock)
                    {
                        Log.LogWarning("[TryAdd Lock] for {Url} failed, continue with existing lock", key);

                        try
                        {
                            await currentLock.WaitAsync();

                            return await CreateImage(imageKey, createImage).ConfigureAwait(false);
                        }
                        finally
                        {
                            currentLock.Release();
                        }
                    }
                    else
                    {
                        var cacheEntry = MemoryCache.CreateEntry(lockKey);
                        currentLock = new SemaphoreSlim(1, 1);
                        cacheEntry.Value = currentLock;

                        return await CreateImage(imageKey, createImage).ConfigureAwait(false);
                    }
                }
            }
            else
            {
                requestedImageLoadsSlowly(false);
            }

            return cachedImage;
        }

        private async Task<T?> CreateImage(string key, Func<Task<T?>> createImage)
        {
            if (!MemoryCache.TryGetValue(key, out T? cacheEntry))
            {
                // Key not in cache, so get data.
                cacheEntry = await createImage();

                // If load failed - remove from cache and return
                // Next load attempt will try to load image again
                if (cacheEntry is null)
                {
                    return null;
                }

                // we rely on the frontend keeping a reference to the provided image,
                // that way we can remove the image from our internal cache
                // after a generous delay any parallel requests should be complete
                MemoryCache.Set(key, cacheEntry, DateTimeOffset.UtcNow.AddSeconds(35));
            }

            return cacheEntry;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                //var locks = _locks.Values.ToList();
                //_locks.Clear();

                //foreach (var lockInstance in locks)
                //{
                //    lockInstance.Dispose();
                //}
            }

            base.Dispose(disposing);
        }
    }
}
