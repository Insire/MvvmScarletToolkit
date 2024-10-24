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
            return Path.Combine(_exeDirectoryPath, "Resources");
        }

        public string GetRawImagesFolderPath()
        {
            return Path.Combine(_exeDirectoryPath, "images", "raw");
        }

        public string GetEncodedImagesFolderPath()
        {
            return Path.Combine(_exeDirectoryPath, "images", "encoded");
        }
    }
}
