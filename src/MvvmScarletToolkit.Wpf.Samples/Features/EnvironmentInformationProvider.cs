using System.IO;

namespace MvvmScarletToolkit.Wpf.Samples.Features
{
    public sealed class EnvironmentInformationProvider
    {
        private readonly string _exeDirectoryPath;

        public EnvironmentInformationProvider()
        {
            using var process = System.Diagnostics.Process.GetCurrentProcess();
            var exeFilePath = process.MainModule!.FileName;

            var exeFileName = Path.GetFileName(exeFilePath);
            _exeDirectoryPath = exeFilePath.Replace(exeFileName, string.Empty);
        }

        public string GetResourceFolderPath()
        {
            var directory = Path.Combine(_exeDirectoryPath, "Resources");

            Directory.CreateDirectory(directory);

            return directory;
        }

        public string GetRawImagesFolderPath()
        {
            var directory = Path.Combine(_exeDirectoryPath, "images", "raw");

            Directory.CreateDirectory(directory);

            return directory;
        }

        public string GetEncodedImagesFolderPath()
        {
            var directory = Path.Combine(_exeDirectoryPath, "images", "encoded");

            Directory.CreateDirectory(directory);

            return directory;
        }

        public string GetLogsFolderPath()
        {
            var directory = Path.Combine(_exeDirectoryPath, "logs");

            Directory.CreateDirectory(directory);

            return directory;
        }
    }
}
