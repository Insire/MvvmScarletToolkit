using Microsoft.Win32;
using System.IO;

namespace MvvmScarletToolkit.Wpf.FileSystemBrowser
{
    public sealed class RegistryFileExtensionMimeTypeResolver : IMimeTypeResolver
    {
        public string? Get(IFileSystemFile fileInfo)
        {
            if (!fileInfo.Exists)
            {
                return null;
            }

            var extension = Path.GetExtension(fileInfo.Name);

            using (var key = Registry.ClassesRoot.OpenSubKey(extension))
            {
                if (key?.GetValue("Content Type") is null)
                {
                    return null;
                }

                return key.GetValue("Content Type").ToString();
            }
        }
    }
}
