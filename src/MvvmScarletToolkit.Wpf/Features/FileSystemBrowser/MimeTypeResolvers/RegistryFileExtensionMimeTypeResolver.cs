using Microsoft.Win32;
using System.IO;

namespace MvvmScarletToolkit.Wpf.FileSystemBrowser
{
#if NET5_0_OR_GREATER
    [System.Runtime.Versioning.SupportedOSPlatform("windows7.0")]
#endif

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
