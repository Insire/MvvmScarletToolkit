using MvvmScarletToolkit.Wpf.Features.FileSystemBrowser.Interfaces;

namespace MvvmScarletToolkit.Wpf.Features.FileSystemBrowser.Files
{
    public interface IFileSystemFile : IFileSystemInfo, IFileSystemChild, IDisposable
    {
        string Extension { get; }
    }
}
