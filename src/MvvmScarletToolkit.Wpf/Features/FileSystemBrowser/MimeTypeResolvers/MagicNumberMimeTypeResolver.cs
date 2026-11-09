using MvvmScarletToolkit.Wpf.Features.FileSystemBrowser.Interfaces;
using System.IO;

namespace MvvmScarletToolkit.Wpf.Features.FileSystemBrowser.MimeTypeResolvers
{
    // https://en.wikipedia.org/wiki/Magic_number_(programming)#Magic_numbers_in_files
    // https://en.wikipedia.org/wiki/List_of_file_signatures
    // https://www.garykessler.net/library/file_sigs.html
    // https://github.com/Muraad/Mime-Detective
    /// <summary>
    /// Detects the mime type from the first bytes of a file. Returns an empty string for unknown content.
    /// </summary>
    public sealed class MagicNumberMimeTypeResolver : IMimeTypeResolver
    {
        public string Get(FileInfo fileInfo)
        {
            var byteBuffer = new byte[256];
            using (var fileStream = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
            {
                if (fileStream.Length >= 256)
                {
                    fileStream.ReadExactly(byteBuffer);
                }
                else
                {
                    _ = fileStream.Read(byteBuffer, 0, (int)fileStream.Length);
                }
            }

            var extension = Path.GetExtension(fileInfo.Name) is null
                ? string.Empty
                : Path.GetExtension(fileInfo.Name).ToUpperInvariant();

            return GetInternal(byteBuffer, extension);
        }

        private static string GetInternal(byte[] file, string extension)
        {
            // checked in order, the first match wins
            foreach (var (isMatch, getMime) in MagicNumbers)
            {
                if (isMatch(file))
                {
                    return getMime(file, extension);
                }
            }

            return string.Empty;
        }

        private static bool StartsWith(byte[] file, byte[] signature, int offset = 0)
        {
            return file.Skip(offset).Take(signature.Length).SequenceEqual(signature);
        }

        private static string GetOggMediaType(string extension)
        {
            return extension switch
            {
                ".OGX" => "application/ogg",
                ".OGA" => "audio/ogg",
                _ => "video/ogg",
            };
        }

        private static string GetRiffMediaType(byte[] file, string extension)
        {
            if (StartsWith(file, WEBP, 8))
            {
                return "image/webp";
            }

            return extension == ".AVI" ? "video/x-msvideo" : "audio/x-wav";
        }

        private static bool IsMp3(byte[] file)
        {
            // tagged files start with an ID3v2 header
            if (file.Take(3).SequenceEqual(ID3))
            {
                return true;
            }

            // untagged files start with an MPEG audio frame header: 11 sync bits, followed by version and layer.
            // the third byte holds bitrate and sample rate, so it differs between files and cannot be part of the signature
            return file[0] == 0xFF && Array.IndexOf(MP3_FRAME_HEADERS, file[1]) >= 0;
        }

        private static string GetIsoMediaType(byte[] brand)
        {
            return System.Text.Encoding.ASCII.GetString(brand) switch
            {
                "heic" or "heix" or "hevc" or "mif1" or "msf1" => "image/heic",
                "avif" or "avis" => "image/avif",
                _ => "video/mp4",
            };
        }

        private static readonly byte[] BMP = [66, 77];
        private static readonly byte[] DOC = [208, 207, 17, 224, 161, 177, 26, 225];
        private static readonly byte[] EXE_DLL = [77, 90];
        private static readonly byte[] GIF = [71, 73, 70, 56];
        private static readonly byte[] ICO = [0, 0, 1, 0];
        private static readonly byte[] JPG = [255, 216, 255];
        private static readonly byte[] ID3 = [73, 68, 51];

        // second byte of an MPEG audio layer III frame header, with and without CRC: MPEG-1, MPEG-2 and MPEG-2.5
        private static readonly byte[] MP3_FRAME_HEADERS = [0xFB, 0xFA, 0xF3, 0xF2, 0xE3, 0xE2];
        private static readonly byte[] OGG = [79, 103, 103, 83, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0];
        private static readonly byte[] PDF = [37, 80, 68, 70, 45, 49, 46];
        private static readonly byte[] PNG = [137, 80, 78, 71, 13, 10, 26, 10, 0, 0, 0, 13, 73, 72, 68, 82];
        private static readonly byte[] RAR = [82, 97, 114, 33, 26, 7, 0];
        private static readonly byte[] SWF = [70, 87, 83];
        private static readonly byte[] TIFF = [73, 73, 42, 0];
        private static readonly byte[] TIFF_BIG_ENDIAN = [77, 77, 0, 42];
        private static readonly byte[] FTYP = [102, 116, 121, 112];
        private static readonly byte[] WEBP = [87, 69, 66, 80];
        private static readonly byte[] TORRENT = [100, 56, 58, 97, 110, 110, 111, 117, 110, 99, 101];
        private static readonly byte[] TTF = [0, 1, 0, 0, 0];
        private static readonly byte[] WAV_AVI = [82, 73, 70, 70];
        private static readonly byte[] WMV_WMA = [48, 38, 178, 117, 142, 102, 207, 17, 166, 217, 0, 170, 0, 98, 206, 108];
        private static readonly byte[] ZIP_DOCX = [80, 75, 3, 4];

        private static readonly (Func<byte[], bool> IsMatch, Func<byte[], string, string> GetMime)[] MagicNumbers =
        [
            (f => StartsWith(f, BMP), (_, _) => "image/bmp"),
            (f => StartsWith(f, DOC), (_, _) => "application/msword"),
            (f => StartsWith(f, EXE_DLL), (_, _) => "application/x-msdownload"), //both use same mime type
            (f => StartsWith(f, GIF), (_, _) => "image/gif"),
            (f => StartsWith(f, ICO), (_, _) => "image/x-icon"),
            (f => StartsWith(f, JPG), (_, _) => "image/jpeg"),
            (IsMp3, (_, _) => "audio/mpeg"),
            (f => StartsWith(f, OGG), (_, extension) => GetOggMediaType(extension)),
            (f => StartsWith(f, PDF), (_, _) => "application/pdf"),
            (f => StartsWith(f, PNG), (_, _) => "image/png"),
            (f => StartsWith(f, RAR), (_, _) => "application/x-rar-compressed"),
            (f => StartsWith(f, SWF), (_, _) => "application/x-shockwave-flash"),
            (f => StartsWith(f, TIFF) || StartsWith(f, TIFF_BIG_ENDIAN), (_, _) => "image/tiff"),
            (f => StartsWith(f, FTYP, 4), (f, _) => GetIsoMediaType(f.Skip(8).Take(4).ToArray())),
            (f => StartsWith(f, TORRENT), (_, _) => "application/x-bittorrent"),
            (f => StartsWith(f, TTF), (_, _) => "application/x-font-ttf"),
            (f => StartsWith(f, WAV_AVI), GetRiffMediaType),
            (f => StartsWith(f, WMV_WMA), (_, extension) => extension == ".WMA" ? "audio/x-ms-wma" : "video/x-ms-wmv"),
            (f => StartsWith(f, ZIP_DOCX), (_, extension) => extension == ".DOCX" ? "application/vnd.openxmlformats-officedocument.wordprocessingml.document" : "application/x-zip-compressed"),
        ];
    }
}
