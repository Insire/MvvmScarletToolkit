using System.IO;

namespace MvvmScarletToolkit.Wpf.Features.FileSystemBrowser
{
    public sealed record ScarletDriveInfo
    {
        public ScarletDriveInfo(DriveInfo info)
        {
            Name = info.Name;
            FullName = info.Name;
            DriveFormat = info.DriveFormat;
            DriveType = info.DriveType;
            AvailableFreeSpace = info.AvailableFreeSpace;
            TotalFreeSpace = info.TotalFreeSpace;
            TotalSize = info.TotalSize;
            IsReady = info.IsReady;
        }

        public ScarletDriveInfo()
        {
            Name = string.Empty;
            FullName = string.Empty;
        }

        public string Name { get; init; }
        public string FullName { get; init; }
        public string? DriveFormat { get; init; }
        public DriveType DriveType { get; init; }
        public long AvailableFreeSpace { get; init; }
        public long TotalFreeSpace { get; init; }
        public long TotalSize { get; init; }
        public bool IsReady { get; init; }
        public bool IsAccessProhibited { get; init; }
    }
}
