using System.Collections.Generic;

namespace MvvmScarletToolkit.FileSystemBrowser
{
    public interface IFileSystemDrive : IFileSystemInfo
    {
        IReadOnlyCollection<IFileSystemInfo> Children { get; }
    }
}
