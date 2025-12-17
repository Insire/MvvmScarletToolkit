using MvvmScarletToolkit.Wpf.Features.FileSystemBrowser.Interfaces;

namespace MvvmScarletToolkit.Wpf.Features.FileSystemBrowser.Directories
{
    public interface IFileSystemDirectory : IFileSystemParent, IFileSystemChild, IDisposable;
}
