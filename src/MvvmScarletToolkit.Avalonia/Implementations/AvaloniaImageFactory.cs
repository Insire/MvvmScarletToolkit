using Avalonia;
using Avalonia.Media.Imaging;
using Microsoft.Extensions.Logging;

namespace MvvmScarletToolkit
{
    public sealed class AvaloniaImageFactory : IImageFactory<Bitmap>
    {
        private readonly ILogger<AvaloniaImageFactory> _logger;
        private readonly SemaphoreSlim _semaphore;

        public AvaloniaImageFactory(ILogger<AvaloniaImageFactory> logger, SemaphoreSlim semaphore)
        {
            _logger = logger;
            _semaphore = semaphore;
        }

        public async Task<Bitmap> FromAsync(Stream stream, ImageSize requestedSize, CancellationToken cancellationToken = default)
        {
            // must not be inside the try block, releasing a semaphore that was never acquired corrupts its count
            await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                var bitmap = new Bitmap(stream);

                return ToResizedBitmap(bitmap, requestedSize);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task ToAsync(Stream stream, Bitmap image, CancellationToken cancellationToken = default)
        {
            await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                image.Save(stream);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Saving Bitmap to stream failed unexpectedly");

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
        private static Bitmap ToResizedBitmap(Bitmap bitmap, ImageSize requestedSize)
        {
            var pixelSize = bitmap.PixelSize;
            if (requestedSize.Width == 0 || requestedSize.Height == 0 || pixelSize.Width == 0 || pixelSize.Height == 0)
            {
                return bitmap;
            }

            var scale = Math.Min((double)requestedSize.Width / pixelSize.Width, (double)requestedSize.Height / pixelSize.Height);
            if (scale >= 1d)
            {
                return bitmap;
            }

            var targetSize = new PixelSize(
                Math.Max(1, (int)Math.Round(pixelSize.Width * scale)),
                Math.Max(1, (int)Math.Round(pixelSize.Height * scale)));

            using (bitmap)
            {
                return bitmap.CreateScaledBitmap(targetSize, BitmapInterpolationMode.HighQuality);
            }
        }
    }
}
