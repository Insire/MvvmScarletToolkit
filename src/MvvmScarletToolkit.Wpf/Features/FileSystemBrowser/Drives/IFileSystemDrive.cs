using System;
using System.IO;

namespace MvvmScarletToolkit
{
    public interface IFileSystemDrive : IFileSystemParent, IDisposable
    {
        string? DriveFormat { get; }
        DriveType DriveType { get; }
        bool IsReady { get; }
        long AvailableFreeSpace { get; }
        long TotalFreeSpace { get; }
        long TotalSize { get; }
    }
}
