using Microsoft.Win32;
using System.IO;

namespace MvvmScarletToolkit.FileSystemBrowser
{
    public sealed class RegistryFileExtensionMimeTypeResolver : IMimeTypeResolver
    {
        public string Get(ScarletFile fileInfo)
        {
            if (!fileInfo.Exists)
            {
                return null;
            }
            var extension = Path.GetExtension(fileInfo.Name);

            var key = Registry.ClassesRoot.OpenSubKey(extension);

            if (key?.GetValue("Content Type") is null)
            {
                return null;
            }

            return key.GetValue("Content Type").ToString();
        }
    }
}
