using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MvvmScarletToolkit.Wpf.Tests.Util
{
    internal static class TestBitmaps
    {
        public static BitmapSource Create(int width, int height)
        {
            var stride = width * 4;
            var pixels = new byte[stride * height];
            Array.Fill(pixels, (byte)0xFF);

            var bitmap = BitmapSource.Create(width, height, 96, 96, PixelFormats.Bgra32, null, pixels, stride);
            bitmap.Freeze();

            return bitmap;
        }

        public static MemoryStream CreatePng(int width, int height)
        {
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(Create(width, height)));

            var stream = new MemoryStream();
            encoder.Save(stream);
            stream.Seek(0, SeekOrigin.Begin);

            return stream;
        }
    }

    /// <summary>
    /// A stream whose async operations complete on another thread, like a real file or network stream may.
    /// </summary>
    internal sealed class YieldingStream(Stream inner) : Stream
    {
        public override bool CanRead => inner.CanRead;
        public override bool CanSeek => inner.CanSeek;
        public override bool CanWrite => inner.CanWrite;
        public override long Length => inner.Length;
        public override long Position { get => inner.Position; set => inner.Position = value; }

        public override async Task FlushAsync(CancellationToken cancellationToken)
        {
            await Task.Yield();
            await inner.FlushAsync(cancellationToken);
        }

        public override void Flush() => inner.Flush();
        public override int Read(byte[] buffer, int offset, int count) => inner.Read(buffer, offset, count);
        public override long Seek(long offset, SeekOrigin origin) => inner.Seek(offset, origin);
        public override void SetLength(long value) => inner.SetLength(value);
        public override void Write(byte[] buffer, int offset, int count) => inner.Write(buffer, offset, count);
    }
}
