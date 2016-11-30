using GraphicsMagick;

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
    }
}
