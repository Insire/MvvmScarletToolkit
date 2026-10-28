using Microsoft.Extensions.Logging;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MvvmScarletToolkit.Wpf
{
    public sealed class WpfImageFactory : IImageFactory<BitmapSource>
    {
        private readonly ILogger<WpfImageFactory> _logger;
        private readonly SemaphoreSlim _semaphore;

        public WpfImageFactory(ILogger<WpfImageFactory> logger, SemaphoreSlim semaphore)
        {
            _logger = logger;
            _semaphore = semaphore;
        }

        public async Task<BitmapSource> FromAsync(Stream stream, ImageSize requestedSize, CancellationToken cancellationToken = default)
        {
            // must not be inside the try block, releasing a semaphore that was never acquired corrupts its count
            await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                // no await between BeginInit and EndInit, the image belongs to the thread that created it
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = stream;
                image.EndInit();
                image.Freeze();

                var bitmapSource = ToResizedBitmapSource(image, requestedSize);
                bitmapSource.Freeze();

                return bitmapSource;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task ToAsync(Stream stream, BitmapSource image, CancellationToken cancellationToken = default)
        {
            await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                var encoder = new PngBitmapEncoder
                {
                    Interlace = PngInterlaceOption.On
                };
                // explicit nulls, the single-argument overload probes the metadata, which throws (and is caught) internally for a BitmapImage
                encoder.Frames.Add(BitmapFrame.Create(image, null, null, null));
                encoder.Save(stream);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Saving BitmapSource to stream failed unexpectedly");

                // callers must know, otherwise a truncated result would be treated as valid
                throw;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <summary>
        /// Scales the image down to fit into the requested size, keeping its aspect ratio. Images are never scaled up.
        /// </summary>
        private static BitmapSource ToResizedBitmapSource(BitmapSource image, ImageSize requestedSize)
        {
            if (requestedSize.Width == 0 || requestedSize.Height == 0 || image.PixelWidth == 0 || image.PixelHeight == 0)
            {
                return image;
            }

            var scale = Math.Min((double)requestedSize.Width / image.PixelWidth, (double)requestedSize.Height / image.PixelHeight);
            if (scale >= 1d)
            {
                return image;
            }

            return new TransformedBitmap(image, new ScaleTransform(scale, scale));
        }
    }
}
