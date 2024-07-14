using System.Diagnostics;
using System.IO;

namespace MvvmScarletToolkit.Wpf.Samples
{
    public sealed class EnvironmentInformationProvider
    {
        private readonly string _exeDirectoryPath;

        public EnvironmentInformationProvider()
        {
            using var process = Process.GetCurrentProcess();
            var exeFilePath = process.MainModule!.FileName;

            var exeFileName = Path.GetFileName(exeFilePath);
            _exeDirectoryPath = exeFilePath.Replace(exeFileName, string.Empty);
        }

        public string GetResourceFolderPath()
        {
            return Path.Combine(_exeDirectoryPath, "Resources");
        }

        public string GetImagesFolderPath()
        {
            return Path.Combine(_exeDirectoryPath, "images");
        }
    }
}
