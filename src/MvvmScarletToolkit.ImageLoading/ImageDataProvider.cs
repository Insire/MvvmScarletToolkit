using Microsoft.Extensions.Logging;
using Microsoft.IO;

namespace MvvmScarletToolkit.ImageLoading
{
    public class ImageDataProvider : IImageDataProvider
    {
        private readonly ILogger<ImageDataProvider> _logger;
        private readonly RecyclableMemoryStreamManager _recyclableMemoryStreamManager;
        private readonly HttpClient _httpClient;

        public ImageDataProvider(
            ILogger<ImageDataProvider> logger,
            RecyclableMemoryStreamManager recyclableMemoryStreamManager,
            HttpClient httpClient)
        {
            _logger = logger;
            _recyclableMemoryStreamManager = recyclableMemoryStreamManager;
            _httpClient = httpClient;
        }

        public async Task<Stream> GetStreamAsync(Uri uri, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Stream.Null;
            }

            var stream = await GetImageStream(uri, cancellationToken);

            if (stream == Stream.Null)
            {
                return stream;
            }

            if (!stream.CanRead)
            {
                return Stream.Null;
            }

            if (stream.CanSeek && stream.Length == 0)
            {
                return Stream.Null;
            }

            await using (stream)
            {
                var resultStream = _recyclableMemoryStreamManager.GetStream();
                await stream.CopyToAsync(resultStream, cancellationToken);

                resultStream.Seek(0, SeekOrigin.Begin);

                return resultStream;
            }
        }

        private async Task<Stream> GetImageStream(Uri uri, CancellationToken cancellationToken = default)
        {
            if (uri.Scheme.Equals("HTTPS", StringComparison.InvariantCultureIgnoreCase) || uri.Scheme.Equals("HTTP", StringComparison.InvariantCultureIgnoreCase))
            {
                try
                {
                    _logger.LogTrace("Fetching stream from {Url}", uri);
                    return await Task.Run(() => _httpClient.GetStreamAsync(uri, cancellationToken));
                }
                catch (TaskCanceledException)
                {
                    return Stream.Null;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Fetching stream from web resource '{Url}' failed", uri);
                    return Stream.Null;
                }
            }

            if (uri.Scheme.Equals("FILE", StringComparison.InvariantCultureIgnoreCase))
            {
                try
                {
                    return await Task.Run(() => File.OpenRead(uri.OriginalString));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Fetching stream from file resource '{Url}' failed", uri);
                    return Stream.Null;
                }
            }

            var imageStream = await GetImageStreamFromChildImplementation(uri, cancellationToken);
            if (imageStream is null)
            {
                throw new NotImplementedException($"{GetType().Name} does not support loading the scheme {uri.Scheme}");
            }

            return imageStream;
        }

        protected virtual Task<Stream> GetImageStreamFromChildImplementation(Uri uri, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Stream.Null);
        }
    }
}
