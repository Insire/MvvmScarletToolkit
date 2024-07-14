using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using MvvmScarletToolkit.Abstractions.ImageLoading;

namespace MvvmScarletToolkit.ImageLoading
{
    /// <summary>
    /// Provides non cached way to asynchronously load images for the gui framework specific ImageLoader
    /// Can be used as base class if you want to create custom caching mechanism
    /// </summary>
    public class BaseWebImageLoader<T> : IAsyncImageLoader<T>
        where T : class
    {
        protected static string CreateLockKey(string key)
        {
            return "lock_" + key;
        }

        protected static string CreateImageKey(string key)
        {
            return "image_" + key;
        }

        protected static string GetKey(Uri uri)
        {
            return uri.OriginalString;
        }

        private readonly bool _shouldDisposeHttpClient;

        protected HttpClient HttpClient { get; }

        protected MemoryCache MemoryCache { get; }

        protected RecyclableMemoryStreamManager StreamManager { get; }

        public IImageFactory<T> ImageFactory { get; }

        protected ILogger<BaseWebImageLoader<T>> Log { get; }

        /// <summary>
        /// Initializes a new instance with new <see cref="HttpClient"/> instance
        /// </summary>
        public BaseWebImageLoader(ILogger<BaseWebImageLoader<T>> log, MemoryCache memoryCache, RecyclableMemoryStreamManager streamManager, IImageFactory<T> imageFactory)
            : this(log, new HttpClient(), memoryCache, streamManager, imageFactory, new BaseWebImageLoaderOptions() { DisposeHttpClient = true })
        {
        }

        /// <summary>
        /// Initializes a new instance with the provided <see cref="HttpClient"/>, and specifies whether that <see cref="HttpClient"/> should be disposed when this instance is disposed.
        /// </summary>
        /// <param name="log"></param>
        /// <param name="httpClient">The HttpMessageHandler responsible for processing the HTTP response messages.</param>
        /// <param name="streamManager"></param>
        /// <param name="imageFactory"></param>
        /// <param name="options"></param>
        public BaseWebImageLoader(ILogger<BaseWebImageLoader<T>> log, HttpClient httpClient, MemoryCache memoryCache, RecyclableMemoryStreamManager streamManager, IImageFactory<T> imageFactory, BaseWebImageLoaderOptions options)
        {
            Log = log;
            HttpClient = httpClient;
            MemoryCache = memoryCache;
            StreamManager = streamManager;
            ImageFactory = imageFactory;
            _shouldDisposeHttpClient = options.DisposeHttpClient;
        }

        /// <inheritdoc />
        public virtual Task<T?> ProvideImageAsync(Uri? url, ImageSize requestedSize, Action<bool> requestedImageLoadsSlowly)
        {
            return LoadAsync(url, requestedSize, requestedImageLoadsSlowly);
        }

        /// <summary>
        /// Attempts to load bitmap
        /// </summary>
        /// <param name="url">Target url</param>
        /// <param name="requestedImageLoadsSlowly"></param>
        /// <returns>Bitmap</returns>
        protected async Task<T?> LoadAsync(Uri? url, ImageSize requestedSize, Action<bool> requestedImageLoadsSlowly)
        {
            if (url == null)
            {
                return null;
            }

            var internalOrCachedBitmap = await LoadFromInternalAsync(url, requestedSize, requestedImageLoadsSlowly).ConfigureAwait(false) ?? await LoadFromGlobalCache(url, requestedSize, requestedImageLoadsSlowly).ConfigureAwait(false);
            if (internalOrCachedBitmap is not null)
            {
                return internalOrCachedBitmap;
            }

            try
            {
                requestedImageLoadsSlowly(true);

                var externalBytes = await LoadDataFromExternalAsync(url, CancellationToken.None).ConfigureAwait(false);
                if (externalBytes is null)
                {
                    return null;
                }

                await using (externalBytes)
                {
                    var bitmap = await Task.Run<T?>(() => ImageFactory.From(externalBytes, requestedSize)).ConfigureAwait(false);

                    await SaveToGlobalCache(url, requestedSize, externalBytes).ConfigureAwait(false);
                    return bitmap;
                }
            }
            catch (Exception ex)
            {
                Log.LogError(ex, "[LOAD] {Url} failed", url);
                return null;
            }
        }

        /// <summary>
        /// Receives image bytes from an internal source (for example, from the disk).
        /// This data will be NOT cached globally (because it is assumed that it is already in internal source and does not require global caching)
        /// </summary>
        protected virtual async Task<T?> LoadFromInternalAsync(Uri? url, ImageSize requestedSize, Action<bool> requestedImageLoadsSlowly)
        {
            if (url?.Scheme.Equals("HTTPS", StringComparison.InvariantCultureIgnoreCase) != false || url.Scheme.Equals("HTTP", StringComparison.InvariantCultureIgnoreCase))
            {
                requestedImageLoadsSlowly(true);
                return null;
            }

            try
            {
                requestedImageLoadsSlowly(false);
                Log.LogTrace("[LOADINTERNAL] {Url}", url);
                return await Task.Run(() => ImageFactory.From(url, requestedSize)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                requestedImageLoadsSlowly(true);
                Log.LogError(ex, "[LOADINTERNAL] {Url} failed", url);
                return null;
            }
        }

        /// <summary>
        /// Receives image bytes from an external source (for example, from the Internet).
        /// This data will be cached globally (if required by the current implementation)
        /// </summary>
        /// <param name="url">Target url</param>
        /// <returns>Image bytes</returns>
        protected virtual async Task<Stream?> LoadDataFromExternalAsync(Uri? url, CancellationToken token = default)
        {
            if (url is null)
            {
                return null;
            }

            if (token.IsCancellationRequested)
            {
                return null;
            }

            try
            {
                return await Task.Run<Stream?>(async () =>
                {
                    if (token.IsCancellationRequested)
                    {
                        return null;
                    }

                    Log.LogTrace("[LOADEXTERNAL] for {Url}", url);
                    var resultStream = StreamManager.GetStream();

                    await using var stream = await HttpClient.GetStreamAsync(url, token);
                    await stream.CopyToAsync(resultStream, token);

                    resultStream.Seek(0, SeekOrigin.Begin);

                    return resultStream;
                }, token).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Log.LogError(ex, "[LOADEXTERNAL] {Url} failed", url);
                return null;
            }
        }

        /// <summary>
        /// Attempts to load image from global cache (if it is stored before)
        /// </summary>
        /// <param name="url">Target url</param>
        /// <param name="requestedImageLoadsSlowly"></param>
        /// <returns>Bitmap</returns>
        protected virtual Task<T?> LoadFromGlobalCache(Uri? url, ImageSize requestedSize, Action<bool> requestedImageLoadsSlowly)
        {
            // Current implementation does not provide global caching

            requestedImageLoadsSlowly(true);

            return Task.FromResult<T?>(null);
        }

        /// <summary>
        /// Attempts to load image from global cache (if it is stored before)
        /// </summary>
        /// <param name="url">Target url</param>
        /// <param name="imageBytes">Bytes to save</param>
        /// <returns>Bitmap</returns>
        protected virtual Task SaveToGlobalCache(Uri? url, ImageSize imageSize, Stream imageBytes)
        {
            // Current implementation does not provide global caching
            return Task.CompletedTask;
        }

        ~BaseWebImageLoader()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && _shouldDisposeHttpClient)
            {
                HttpClient.Dispose();
            }
        }
    }
}
