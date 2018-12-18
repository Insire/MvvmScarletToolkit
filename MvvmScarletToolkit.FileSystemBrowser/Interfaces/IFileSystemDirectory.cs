using System.Collections.Generic;

namespace MvvmScarletToolkit.FileSystemBrowser
{
    public interface IFileSystemDirectory : IFileSystemInfo
    {
        IReadOnlyCollection<IFileSystemInfo> Children { get; }
    }
}
