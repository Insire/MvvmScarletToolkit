using MvvmScarletToolkit.Abstractions.ImageLoading;
using System.IO;
using System;
using System.Windows.Media.Imaging;
using ImageMagick;

namespace MvvmScarletToolkit.Wpf.Samples
{
    public sealed class ImageFactory : IImageFactory<BitmapSource>
    {
        public BitmapSource From(Stream stream, ImageSize requestedSize)
        {
            var img = Resize(new MagickImage(stream), requestedSize);
            img.Freeze();

            return img;
        }

        public BitmapSource From(Uri uri, ImageSize requestedSize)
        {
            var img = Resize(new MagickImage(uri.OriginalString), requestedSize);
            img.Freeze();

            return img;
        }

        public BitmapSource From(string uri, ImageSize requestedSize)
        {
            var img = Resize(new MagickImage(uri), requestedSize);
            img.Freeze();

            return img;
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
