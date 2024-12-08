using ImageMagick;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace MvvmScarletToolkit.Wpf.Samples.Features.Image
{
    public sealed class ImageFactory : IImageFactory<BitmapSource>
    {
        private readonly ILogger<ImageFactory> _logger;
        private readonly SemaphoreSlim _semaphore;

        public ImageFactory(ILogger<ImageFactory> logger, SemaphoreSlim semaphore)
        {
            _logger = logger;
            _semaphore = semaphore;
        }

        public async Task<BitmapSource> FromAsync(Stream stream, ImageSize requestedSize, CancellationToken cancellationToken = default)
        {
            try
            {
                await _semaphore.WaitAsync(cancellationToken);

                var img = Resize(new MagickImage(stream), requestedSize);
                img.Freeze();

                return img;
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

        private static BitmapSource Resize(MagickImage magickImage, ImageSize requestedSize)
        {
            // Read from file
            using (magickImage)
            {
                var size = new MagickGeometry(requestedSize.Width, requestedSize.Height)
                {
                    Less = false,
                    Greater = true,

                    // This will resize the image to a fixed size without maintaining the aspect ratio.
                    // Normally an image will be resized to fit inside the specified size.
                    IgnoreAspectRatio = false
                };

                magickImage.Resize(size);

                // Save the result
                return magickImage.ToBitmapSource();
            }
        }
    }
}
