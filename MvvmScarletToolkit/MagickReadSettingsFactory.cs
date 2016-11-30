using GraphicsMagick;
using System.IO;

namespace MvvmScarletToolkit
{
    public static class MagickReadSettingsFactory
    {
        public static MagickReadSettings GetSettings(string path)
        {
            var extension = Path.GetExtension(path).ToLowerInvariant();

            switch (extension)
            {
                case ".jpg":
                case ".jpeg":
                    return GetJPGSettings();

                case ".png":
                    return GetPNGSettings();

                case ".gif":
                    return GetGIFSettings();

                case ".ico":
                    return GetICONSettings();

                case ".icon":
                    return GetICOSettings();

                case ".tif":
                case ".tiff":
                    return GetTIFSettings();

                case ".bmp":
                    return GetBMPSettings();

                case ".svg":
                    return GetSVGSettings();
                default:
                    return new MagickReadSettings();

            }
        }

        internal static MagickReadSettings GetPNGSettings()
        {
            return new MagickReadSettings
            {
                Format = MagickFormat.Png,
            };
        }

        internal static MagickReadSettings GetJPGSettings()
        {
            return new MagickReadSettings
            {
                Format = MagickFormat.Jpeg,
            };
        }

        internal static MagickReadSettings GetGIFSettings()
        {
            return new MagickReadSettings
            {
                Format = MagickFormat.Gif,
            };
        }

        internal static MagickReadSettings GetICOSettings()
        {
            return new MagickReadSettings
            {
                Format = MagickFormat.Ico,
            };
        }

        internal static MagickReadSettings GetICONSettings()
        {
            return new MagickReadSettings
            {
                Format = MagickFormat.Icon,
            };
        }

        internal static MagickReadSettings GetTIFSettings()
        {
            return new MagickReadSettings
            {
                Format = MagickFormat.Tif,
            };
        }

        internal static MagickReadSettings GetBMPSettings()
        {
            return new MagickReadSettings
            {
                Format = MagickFormat.Bmp,
            };
        }

        internal static MagickReadSettings GetSVGSettings()
        {
            return new MagickReadSettings
            {
                Format = MagickFormat.Svg,
            };
        }
    }
}
