using Microsoft.Extensions.Logging;
using Microsoft.IO;
using System.Text;

namespace MvvmScarletToolkit.ImageLoading
{
    public sealed class ImageDataFileystemCache : IImageDataFileystemCache
    {
        private readonly ILogger<ImageDataFileystemCache> _logger;
        private readonly RecyclableMemoryStreamManager _recyclableMemoryStreamManager;
        private readonly ImageDataFileystemCacheOptions _options;

        public ImageDataFileystemCache(
            ILogger<ImageDataFileystemCache> logger,
            RecyclableMemoryStreamManager recyclableMemoryStreamManager,
            ImageDataFileystemCacheOptions options)
        {
            _logger = logger;
            _recyclableMemoryStreamManager = recyclableMemoryStreamManager;
            _options = options;

            if (options.CreateFolder)
            {
                Directory.CreateDirectory(options.CacheDirectoryPath);
            }
        }

        /// <inheritdoc />
        public async Task<Stream?> GetStreamAsync(Uri uri, ImageSize requestedSize, CancellationToken cancellationToken = default)
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
                _logger.LogDebug("Resource with {Key} was found on disk", key);

                return await Task.Run(() => File.OpenRead(path), cancellationToken).ConfigureAwait(false);
            }

            _logger.LogDebug("Resource with {Key} could not be found on disk", key);

            return null;
        }

        /// <inheritdoc />
        public async Task CacheStreamAsync(Stream imageDataStream, Uri uri, ImageSize requestedSize, CancellationToken cancellationToken = default)
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

            imageDataStream.Seek(0, SeekOrigin.Begin);

            var path = Path.Combine(_options.CacheDirectoryPath, key);
            await using (var fileStream = File.OpenWrite(path))
            {
                await imageDataStream.CopyToAsync(fileStream, cancellationToken).ConfigureAwait(false);
            }

            _logger.LogDebug("Resource with {Key} has been cached on disk", key);
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
    }
}
