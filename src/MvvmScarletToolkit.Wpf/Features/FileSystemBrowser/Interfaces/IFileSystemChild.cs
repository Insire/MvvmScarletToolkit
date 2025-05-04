using System;

namespace MvvmScarletToolkit
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
