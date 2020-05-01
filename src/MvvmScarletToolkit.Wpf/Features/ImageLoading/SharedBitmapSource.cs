using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MvvmScarletToolkit.Wpf.Features.ImageLoading
{
    // source: https://stackoverflow.com/questions/94456/load-a-wpf-bitmapimage-from-a-system-drawing-bitmap/7390373#7390373
    [System.Diagnostics.CodeAnalysis.SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP007:Don't dispose injected.",
        Justification = "SharedBitmapSource is a container around the injected bitmap instance that provides a bitmapsource without creating a new instance of it")]
    public sealed class SharedBitmapSource : BitmapSource, IDisposable
    {
        private bool _disposed = false;

        public Bitmap Bitmap { get; }

        public override double DpiX => Bitmap.HorizontalResolution;

        public override double DpiY => Bitmap.VerticalResolution;

        public override int PixelHeight => Bitmap.Height;

        public override int PixelWidth => Bitmap.Width;

        public override System.Windows.Media.PixelFormat Format => ConvertPixelFormat(Bitmap.PixelFormat);

        public override BitmapPalette? Palette => null;

        public SharedBitmapSource(int width, int height, System.Drawing.Imaging.PixelFormat sourceFormat)
            : this(new Bitmap(width, height, sourceFormat))
        {
        }

        public SharedBitmapSource(Bitmap bitmap)
        {
            Bitmap = bitmap ?? throw new ArgumentNullException(nameof(bitmap));
        }

        ~SharedBitmapSource()
        {
            Dispose(false);
        }

        public override void CopyPixels(Int32Rect sourceRect, Array pixels, int stride, int offset)
        {
            var sourceData = Bitmap.LockBits(new Rectangle(sourceRect.X, sourceRect.Y, sourceRect.Width, sourceRect.Height), ImageLockMode.ReadOnly, Bitmap.PixelFormat);

            var length = sourceData.Stride * sourceData.Height;

            if (pixels is byte[] bytes)
            {
                Marshal.Copy(sourceData.Scan0, bytes, 0, length);
            }

            Bitmap.UnlockBits(sourceData);
        }

        protected override Freezable? CreateInstanceCore()
        {
            var instance = Activator.CreateInstance(GetType());
            if (instance is Freezable freezable)
                return freezable;

            return default;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private static System.Windows.Media.PixelFormat ConvertPixelFormat(System.Drawing.Imaging.PixelFormat sourceFormat)
        {
            return sourceFormat switch
            {
                System.Drawing.Imaging.PixelFormat.Format24bppRgb => PixelFormats.Bgr24,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb => PixelFormats.Pbgra32,
                System.Drawing.Imaging.PixelFormat.Format32bppRgb => PixelFormats.Bgr32,
                _ => new System.Windows.Media.PixelFormat(),
            };
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Free other state (managed objects).
                    Bitmap.Dispose();
                }
                // Free your own state (unmanaged objects).
                // Set large fields to null.
                _disposed = true;
            }
        }
    }
}
