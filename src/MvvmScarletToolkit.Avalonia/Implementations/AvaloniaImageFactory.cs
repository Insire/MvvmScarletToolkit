using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Microsoft.Extensions.Logging;
using SkiaSharp;
using System.Runtime.CompilerServices;

namespace MvvmScarletToolkit
{
    public sealed class AvaloniaImageFactory : IImageFactory<Bitmap>
    {
        private const int JpegQuality = 90;

        private static readonly object _opaque = new();

        private readonly ILogger<AvaloniaImageFactory> _logger;
        private readonly SemaphoreSlim _semaphore;
        private readonly PngBitmapEncoderOptions _pngOptions;
        private readonly JpegBitmapEncoderOptions _jpegOptions;

        // scaled bitmaps always report AlphaFormat.Premul, so whether an image is opaque has to be remembered from its source.
        // Anything not in here is saved as png, which is always safe
        private readonly ConditionalWeakTable<Bitmap, object> _opaqueImages;

        public AvaloniaImageFactory(ILogger<AvaloniaImageFactory> logger, SemaphoreSlim semaphore)
        {
            _logger = logger;
            _semaphore = semaphore;
            _pngOptions = new PngBitmapEncoderOptions() { CompressionLevel = System.IO.Compression.CompressionLevel.Optimal };
            _jpegOptions = new JpegBitmapEncoderOptions() { Quality = JpegQuality };
            _opaqueImages = [];
        }

        public async Task<Bitmap> FromAsync(Stream stream, ImageSize requestedSize, CancellationToken cancellationToken = default)
        {
            // must not be inside the try block, releasing a semaphore that was never acquired corrupts its count
            await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                var source = TryReadImageInfo(stream);
                var bitmap = Decode(stream, source, requestedSize, out var isOpaque);
                if (isOpaque)
                {
                    _opaqueImages.AddOrUpdate(bitmap, _opaque);
                }

                return bitmap;
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
                image.Save(stream, IsOpaque(image) ? _jpegOptions : _pngOptions);
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
        /// JPEG is smaller and faster to encode and decode, but it can't store transparency.
        /// </summary>
        private bool IsOpaque(Bitmap image)
        {
            return image.AlphaFormat == AlphaFormat.Opaque || _opaqueImages.TryGetValue(image, out _);
        }

        /// <summary>
        /// <see cref="Bitmap"/> can't tell the size or transparency of an image without decoding it, the header is enough for that.
        /// </summary>
        /// <returns><see langword="null"/> when <paramref name="stream"/> can't be read twice or doesn't contain a known format</returns>
        private static SKImageInfo? TryReadImageInfo(Stream stream)
        {
            if (!stream.CanSeek)
            {
                return null;
            }

            var position = stream.Position;

            SKImageInfo? info;

            // the codec owns the wrapper, which leaves the stream of the caller open
            using (var codec = SKCodec.Create(new SKManagedStream(stream, false)))
            {
                info = codec?.Info;
            }

            stream.Position = position;

            return info;
        }

        /// <summary>
        /// Decoding directly to the size that is needed is a lot faster than decoding the full image and scaling it afterwards.
        /// </summary>
        private static Bitmap Decode(Stream stream, SKImageInfo? source, ImageSize requestedSize, out bool isOpaque)
        {
            isOpaque = source?.AlphaType == SKAlphaType.Opaque;

            if (source is { } info)
            {
                var scale = GetScale(info.Width, info.Height, requestedSize);

                // decoding rounds down, which would leave nothing of a very thin image
                if (scale < 1d && (int)(info.Width * scale) > 0 && (int)(info.Height * scale) > 0)
                {
                    return requestedSize.Width * (double)info.Height <= requestedSize.Height * (double)info.Width
                        ? Bitmap.DecodeToWidth(stream, (int)requestedSize.Width, BitmapInterpolationMode.HighQuality)
                        : Bitmap.DecodeToHeight(stream, (int)requestedSize.Height, BitmapInterpolationMode.HighQuality);
                }
            }

            var bitmap = new Bitmap(stream);

            isOpaque |= bitmap.AlphaFormat == AlphaFormat.Opaque;

            return ToResizedBitmap(bitmap, requestedSize);
        }

        /// <returns>1 when the image doesn't have to be scaled down</returns>
        private static double GetScale(int width, int height, ImageSize requestedSize)
        {
            if (requestedSize.Width == 0 || requestedSize.Height == 0 || width == 0 || height == 0)
            {
                return 1d;
            }

            return Math.Min((double)requestedSize.Width / width, (double)requestedSize.Height / height);
        }

        /// <summary>
        /// Scales the image down to fit into the requested size, keeping its aspect ratio. Images are never scaled up.
        /// </summary>
        private static Bitmap ToResizedBitmap(Bitmap bitmap, ImageSize requestedSize)
        {
            var pixelSize = bitmap.PixelSize;

            var scale = GetScale(pixelSize.Width, pixelSize.Height, requestedSize);
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
