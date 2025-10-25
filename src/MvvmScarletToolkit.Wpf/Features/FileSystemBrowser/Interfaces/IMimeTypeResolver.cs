using System.IO;

namespace MvvmScarletToolkit.Wpf.Features.FileSystemBrowser.Interfaces
{
    public interface IMimeTypeResolver
    {
        string? Get(FileInfo fileInfo);
    }
}
