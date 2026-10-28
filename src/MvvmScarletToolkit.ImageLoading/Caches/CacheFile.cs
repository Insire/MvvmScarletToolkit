using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace MvvmScarletToolkit.ImageLoading.Caches
{
    internal static class CacheFile
    {
        private const string TempExtension = ".tmp";
        private const int BufferSize = 4096;

        /// <summary>
        /// Creates a file name that is unique for the combination of scope, uri and size.
        /// </summary>
        public static string CreateName(string scope, Uri uri, ImageSize size)
        {
            var bytes = Encoding.UTF8.GetBytes($"{scope}_'{uri.OriginalString}'_w{size.Width}_h{size.Height}");

            return Convert.ToHexString(MD5.HashData(bytes));
        }

        /// <summary>
        /// Opens a file for reading without blocking writers of the same file.
        /// </summary>
        public static FileStream OpenRead(string path)
        {
            return new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete, BufferSize, FileOptions.Asynchronous | FileOptions.SequentialScan);
        }

        /// <summary>
        /// Opens a file for reading, <see langword="null"/> when it does not exist.
        /// </summary>
        /// <remarks>
        /// A cache miss is the common case, checking first avoids throwing (and a debugger stopping at) an exception for every one of them.
        /// </remarks>
        public static FileStream? TryOpenRead(string path)
        {
            if (!File.Exists(path))
            {
                return null;
            }

            try
            {
                return OpenRead(path);
            }
            catch (Exception ex) when (ex is FileNotFoundException or DirectoryNotFoundException)
            {
                // removed in between
                return null;
            }
        }

        /// <summary>
        /// Writes to a temporary file first and moves it into place once complete,
        /// so readers never observe a partially written file.
        /// </summary>
        /// <returns><see langword="false"/> when an existing file could not be replaced and was kept instead.</returns>
        public static async Task<bool> WriteAtomicAsync(string path, Func<Stream, Task> write, CancellationToken token)
        {
            var tempPath = $"{path}.{Guid.NewGuid():N}{TempExtension}";

            try
            {
                await using (var stream = new FileStream(tempPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, BufferSize, FileOptions.Asynchronous))
                {
                    await write(stream).ConfigureAwait(false);
                }

                token.ThrowIfCancellationRequested();

                if (new FileInfo(tempPath).Length == 0)
                {
                    throw new InvalidDataException("Nothing was written to the cache file.");
                }

                try
                {
                    File.Move(tempPath, path, overwrite: true);
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException && File.Exists(path))
                {
                    // Windows refuses to replace a file that is currently open, the existing file is complete though
                    TryDelete(tempPath);

                    return false;
                }

                return true;
            }
            catch
            {
                TryDelete(tempPath);
                throw;
            }
        }

        public static void TryDelete(string path)
        {
            try
            {
                File.Delete(path);
            }
            catch
            {
                // best effort
            }
        }

        public static void Initialize(ILogger logger, string directory, bool createFolder, bool clearDirectory, int maxFileAgeInDays)
        {
            if (!createFolder)
            {
                return;
            }

            if (clearDirectory)
            {
                try
                {
                    if (Directory.Exists(directory))
                    {
                        Directory.Delete(directory, true);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Clearing cache directory {Directory} failed", directory);
                }
            }

            Directory.CreateDirectory(directory);

            if (!clearDirectory)
            {
                _ = Task.Run(() => RemoveStaleFiles(logger, directory, maxFileAgeInDays));
            }
        }

        private static void RemoveStaleFiles(ILogger logger, string directory, int maxFileAgeInDays)
        {
            try
            {
                var now = DateTime.UtcNow;
                var tempThreshold = now.AddHours(-1);
                var maxAgeThreshold = maxFileAgeInDays > 0 ? now.AddDays(-maxFileAgeInDays) : (DateTime?)null;

                foreach (var file in Directory.EnumerateFiles(directory))
                {
                    try
                    {
                        var lastWrite = File.GetLastWriteTimeUtc(file);
                        var isTemp = file.EndsWith(TempExtension, StringComparison.OrdinalIgnoreCase);

                        if ((isTemp && lastWrite < tempThreshold) || (maxAgeThreshold is not null && lastWrite < maxAgeThreshold))
                        {
                            File.Delete(file);
                        }
                    }
                    catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
                    {
                        // in use or protected, try again on next start
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Removing stale cache files from {Directory} failed", directory);
            }
        }
    }
}
