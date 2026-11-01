namespace MvvmScarletToolkit.Wpf.Features.FileSystemBrowser
{
    /// <summary>
    /// Maps mime types to the names shown in the browser.
    /// </summary>
    public static class MimeTypeNames
    {
        private static readonly Dictionary<string, string> _names = new(StringComparer.OrdinalIgnoreCase)
        {
            ["application/msword"] = "Word document",
            ["application/pdf"] = "PDF document",
            ["application/vnd.openxmlformats-officedocument.wordprocessingml.document"] = "Word document",
            ["application/x-msdownload"] = "Windows executable",
            ["application/x-rar-compressed"] = "RAR archive",
            ["application/x-zip-compressed"] = "ZIP archive",
            ["application/zip"] = "ZIP archive",
            ["audio/mpeg"] = "MP3 audio",
            ["audio/x-wav"] = "WAV audio",
            ["image/avif"] = "AVIF image",
            ["image/bmp"] = "BMP image",
            ["image/gif"] = "GIF image",
            ["image/heic"] = "HEIF image",
            ["image/jpeg"] = "JPEG image",
            ["image/png"] = "PNG image",
            ["image/tiff"] = "TIFF image",
            ["image/webp"] = "WebP image",
            ["image/x-icon"] = "Icon",
            ["text/plain"] = "Text",
            ["video/mp4"] = "MP4 video",
            ["video/x-msvideo"] = "AVI video",
        };

        public static string Get(string mimeType, string extension)
        {
            if (_names.TryGetValue(mimeType, out var name))
            {
                return name;
            }

            if (extension.Length > 1)
            {
                return extension.TrimStart('.').ToUpperInvariant() + " file";
            }

            return string.IsNullOrEmpty(mimeType) ? "File" : mimeType;
        }
    }
}
