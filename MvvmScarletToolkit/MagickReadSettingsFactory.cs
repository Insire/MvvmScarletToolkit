using GraphicsMagick;
using System.IO;

namespace MvvmScarletToolkit
{
    public static class MagickReadSettingsFactory
    {
        public static MagickReadSettings GetSettings(string path)
        {
            var info = new MagickImageInfo(path);

            return new MagickReadSettings
            {
                Format = info.Format,
                ColorSpace = info.ColorSpace,
                Height = info.Height,
                Width = info.Width,
            };
        }

        public static MagickReadSettings GetSettings(Stream stream)
        {
            var info = new MagickImageInfo(stream);

            return new MagickReadSettings
            {
                Format = info.Format,
                ColorSpace = info.ColorSpace,
                Height = info.Height,
                Width = info.Width,
            };
        }
    }
}
