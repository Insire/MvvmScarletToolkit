using Microsoft.Win32;
using MvvmScarletToolkit.Wpf.Features.FileSystemBrowser.Interfaces;
using System.IO;

namespace MvvmScarletToolkit.Wpf.Features.FileSystemBrowser.MimeTypeResolvers
{
    public sealed class RegistryFileExtensionMimeTypeResolver : IMimeTypeResolver
    {
        public string? Get(FileInfo fileInfo)
        {
            if (!fileInfo.Exists)
            {
                return null;
            }

            var extension = Path.GetExtension(fileInfo.Name);

            using (var key = Registry.ClassesRoot.OpenSubKey(extension))
            {
                if (key is null)
                {
                    return null;
                }

                if (key.GetValue("Content Type") is null)
                {
                    return null;
                }

                return key.GetValue("Content Type")?.ToString();
            }
        }
    }
}
