using Microsoft.Extensions.Logging;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MvvmScarletToolkit.Wpf
{
    public sealed class WpfImageFactory : IImageFactory<BitmapSource>
    {
        private const int JpegQuality = 90;

        // listed instead of excluding formats with alpha, the palette of an indexed format can contain transparency
        private static readonly HashSet<PixelFormat> _opaqueFormats = [PixelFormats.Bgr24, PixelFormats.Bgr32, PixelFormats.Rgb24, PixelFormats.Gray8];

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
                var (decodeWidth, decodeHeight) = GetDecodeSize(stream, requestedSize);

                // no await between BeginInit and EndInit, the image belongs to the thread that created it
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.DecodePixelWidth = decodeWidth;
                image.DecodePixelHeight = decodeHeight;
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
                var encoder = CreateEncoder(image);
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
        /// JPEG is smaller and faster to encode and decode, but it can't store transparency.
        /// </summary>
        private static BitmapEncoder CreateEncoder(BitmapSource image)
        {
            return _opaqueFormats.Contains(image.Format)
                ? new JpegBitmapEncoder { QualityLevel = JpegQuality }
                : new PngBitmapEncoder { Interlace = PngInterlaceOption.Off };
        }

        /// <summary>
        /// Decoding directly to the size that is needed is a lot faster than decoding the full image and scaling it afterwards.
        /// Setting one side keeps the aspect ratio, so this is the side that limits the size.
        /// </summary>
        /// <returns>0 for a side that is not limited</returns>
        private static (int Width, int Height) GetDecodeSize(Stream stream, ImageSize requestedSize)
        {
            if (requestedSize.Width == 0 || requestedSize.Height == 0 || !stream.CanSeek)
            {
                return (0, 0);
            }

            var position = stream.Position;
            var frame = BitmapDecoder.Create(stream, BitmapCreateOptions.DelayCreation, BitmapCacheOption.None).Frames[0];
            var (width, height) = (frame.PixelWidth, frame.PixelHeight);
            stream.Position = position;

            var widthScale = (double)requestedSize.Width / width;
            var heightScale = (double)requestedSize.Height / height;

            if (Math.Min(widthScale, heightScale) >= 1d)
            {
                return (0, 0);
            }

            return widthScale <= heightScale
                ? ((int)requestedSize.Width, 0)
                : (0, (int)requestedSize.Height);
        }

        /// <summary>
        /// Scales the image down to fit into the requested size, keeping its aspect ratio. Images are never scaled up.
        /// </summary>
        /// <remarks>
        /// Only has something to do when the size could not be limited while decoding.
        /// </remarks>
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

            // a TransformedBitmap keeps the full size source alive, copying the scaled pixels lets it be collected
            return new WriteableBitmap(new TransformedBitmap(image, new ScaleTransform(scale, scale)));
        }
    }
}
