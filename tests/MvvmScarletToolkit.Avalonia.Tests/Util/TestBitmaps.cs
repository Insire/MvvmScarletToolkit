using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System.Runtime.InteropServices;

namespace MvvmScarletToolkit.Avalonia.Tests.Util
{
    internal static class TestBitmaps
    {
        public static Bitmap Create(int width, int height)
        {
            var stride = width * 4;
            var pixels = new byte[stride * height];
            Array.Fill(pixels, (byte)0xFF);

            var handle = GCHandle.Alloc(pixels, GCHandleType.Pinned);
            try
            {
                return new Bitmap(PixelFormats.Bgra8888, AlphaFormat.Premul, handle.AddrOfPinnedObject(), new PixelSize(width, height), new Vector(96, 96), stride);
            }
            finally
            {
                handle.Free();
            }
        }

        public static MemoryStream CreatePng(int width, int height)
        {
            using var bitmap = Create(width, height);

            var stream = new MemoryStream();
            bitmap.Save(stream);
            stream.Seek(0, SeekOrigin.Begin);

            return stream;
        }
    }
}
