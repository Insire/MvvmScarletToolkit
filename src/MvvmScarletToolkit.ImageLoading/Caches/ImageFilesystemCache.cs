using Microsoft.Extensions.Logging;
using Microsoft.IO;
using MvvmScarletToolkit.ImageLoading;
using System.Runtime.CompilerServices;
using System.Text;

namespace MvvmScarletToolkit.Wpf.Samples
{
    public sealed class ImageFilesystemCache<TImage> : IImageFilesystemCache<TImage>
        where TImage : class
    {
        public sealed class ImageFileSemaphore : SemaphoreSlim
        {
            public ImageFileSemaphore()
                : base(1, 1)
            {
            }
        }

        private readonly ILogger<ImageFilesystemCache<TImage>> _logger;
        private readonly IImageFactory<TImage> _imageFactory;
        private readonly RecyclableMemoryStreamManager _recyclableMemoryStreamManager;
        private readonly ImageFilesystemCacheOptions _options;
        private readonly ConditionalWeakTable<string, ImageFileSemaphore> _lockDictionary;

        public ImageFilesystemCache(
            ILogger<ImageFilesystemCache<TImage>> logger,
            IImageFactory<TImage> imageFactory,
            RecyclableMemoryStreamManager recyclableMemoryStreamManager,
            ImageFilesystemCacheOptions options)
        {
            _logger = logger;
            _imageFactory = imageFactory;
            _recyclableMemoryStreamManager = recyclableMemoryStreamManager;
            _options = options;
            _lockDictionary = new ConditionalWeakTable<string, ImageFileSemaphore>();

            if (!options.CreateFolder)
            {
                return;
            }

            if (options.ClearCacheDirectoryOnInit)
            {
                Directory.CreateDirectory(options.CacheDirectoryPath);
                Directory.Delete(options.CacheDirectoryPath, true);
            }

            Directory.CreateDirectory(options.CacheDirectoryPath);
        }

        /// <inheritdoc />
        public async Task<TImage?> GetImageAsync(Uri uri, ImageSize requestedSize, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return null;
            }

            if (_options.IsEnabled == false)
            {
                return null;
            }

            var key = await CreateKey(uri, requestedSize, cancellationToken).ConfigureAwait(false);
            var path = Path.Combine(_options.CacheDirectoryPath, key);
            if (File.Exists(path))
            {
                _logger.LogDebug("Image with {Key} was found on disk at {Path}", key, path);

                return await Task.Run(() => ReadImage(path, requestedSize, cancellationToken), cancellationToken).ConfigureAwait(false);
            }

            _logger.LogDebug("Image with {Key} could not be found on disk at {Path}", key, path);

            return null;
        }

        /// <inheritdoc />
        public async Task CacheImageAsync(TImage image, Uri uri, ImageSize requestedSize, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            if (_options.IsEnabled == false)
            {
                return;
            }

            var key = await CreateKey(uri, requestedSize, cancellationToken).ConfigureAwait(false);

            var path = Path.Combine(_options.CacheDirectoryPath, key);
            try
            {
                await WriteImage(path, image, cancellationToken).ConfigureAwait(false);

                _logger.LogDebug("Image with {Key} has been cached on disk at {Path}", key, path);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Opening file {FilePath} failed unexpectedly", path);
            }
        }

        private async Task<string> CreateKey(Uri? input, ImageSize imageSize, CancellationToken cancellationToken)
        {
            if (input is null)
            {
                return string.Empty;
            }

            var key = $"'{input.OriginalString}'_w{imageSize.Width}_h{imageSize.Height}";

            var bytes = Encoding.ASCII.GetBytes(key);

            await using var resultStream = _recyclableMemoryStreamManager.GetStream(null, bytes.Length);
            resultStream.Write(bytes, 0, bytes.Length);
            resultStream.Seek(0, SeekOrigin.Begin);

            return await resultStream.CalculateMd5Async(token: cancellationToken).ConfigureAwait(false);
        }

        private async Task<TImage> ReadImage(string path, ImageSize requestedSize, CancellationToken token)
        {
            var semaphore = _lockDictionary.GetOrCreateValue(path);

            try
            {
                await semaphore.WaitAsync(token);

                await using (var stream = File.OpenRead(path))
                {
                    return await _imageFactory.FromAsync(stream, requestedSize, token).ConfigureAwait(false);
                }
            }
            finally
            {
                semaphore.Release();
            }
        }

        private async Task WriteImage(string path, TImage image, CancellationToken token)
        {
            var semaphore = _lockDictionary.GetOrCreateValue(path);

            try
            {
                await semaphore.WaitAsync(token);

                await using (var fileStream = File.OpenWrite(path))
                {
                    await Task.Run(() => _imageFactory.ToAsync(fileStream, image, token), token).ConfigureAwait(false);
                }
            }
            finally
            {
                semaphore.Release();
            }
        }
    }
}
