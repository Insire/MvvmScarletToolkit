using System;

namespace MvvmScarletToolkit.Wpf.Features.FileSystemBrowser.Interfaces
{
    public interface IFileSystemChild : IFileSystemInfo
    {
        DateTime? LastWriteTimeUtc { get; }
        DateTime? LastAccessTimeUtc { get; }
        DateTime? CreationTimeUtc { get; }
        bool IsHidden { get; }
        IFileSystemParent? Parent { get; }
    }
}
