using System.IO;

namespace MvvmScarletToolkit.Wpf.Features.FileSystemBrowser
{
    public static class FileExtensions
    {
        private const byte CR = (byte)'\r';
        private const byte LF = (byte)'\n';
        private const byte NULL = 0;

        // source: https://stackoverflow.com/questions/119559/determine-the-number-of-lines-within-a-text-file/50508830#50508830
        /// <summary>
        /// Counts the lines of a text stream. The first line break found decides, whether lines end with <c>\n</c> or <c>\r</c>.
        /// </summary>
        public static long CountLinesMaybe(this Stream stream)
        {
            var lineCount = 0L;

            var byteBuffer = new byte[1024 * 1024];
            var detectedEOL = NULL;
            var lastByte = NULL;

            int bytesRead;
            while ((bytesRead = stream.Read(byteBuffer, 0, byteBuffer.Length)) > 0)
            {
                ReadOnlySpan<byte> chunk = byteBuffer.AsSpan(0, bytesRead);
                lastByte = chunk[^1];

                if (detectedEOL == NULL)
                {
                    var index = chunk.IndexOfAny(LF, CR);
                    if (index < 0)
                    {
                        continue;
                    }

                    detectedEOL = chunk[index];
                    lineCount++;
                    chunk = chunk[(index + 1)..];
                }

                lineCount += chunk.Count(detectedEOL);
            }

            // the last line has no line break of its own
            if (lastByte != LF && lastByte != CR && lastByte != NULL)
            {
                lineCount++;
            }

            return lineCount;
        }
    }
}
