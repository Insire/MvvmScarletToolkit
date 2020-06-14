using MvvmScarletToolkit.Abstractions;
using System.IO;

namespace MvvmScarletToolkit
{
    public interface IFileSystemDrive : IFileSystemParent
    {
        string? DriveFormat { get; }
        DriveType DriveType { get; }
        bool IsReady { get; }
        long AvailableFreeSpace { get; }
        long TotalFreeSpace { get; }
        long TotalSize { get; }
    }
}
