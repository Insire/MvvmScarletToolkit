using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
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
            try
            {
                await _semaphore.WaitAsync(cancellationToken);
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = stream;
                image.StreamSource.Flush();
                image.EndInit();
                image.Freeze();

                image.StreamSource.Dispose();

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
            try
            {
                await _semaphore.WaitAsync(cancellationToken);
                var encoder = new PngBitmapEncoder
                {
                    Interlace = PngInterlaceOption.On
                };
                encoder.Frames.Add(BitmapFrame.Create(image));
                encoder.Save(stream);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Saving BitmapSource to stream failed unexpectedly");
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private static BitmapSource ToResizedBitmapSource(BitmapImage image, ImageSize requestedSize)
        {
            if (image.Width != requestedSize.Width || image.Height != requestedSize.Height)
            {
                return new TransformedBitmap(image, new ScaleTransform(0.5, 0.5));
            }

            // convert
            return image;
        }
    }
}
