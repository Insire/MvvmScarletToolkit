using System;
using System.IO;

namespace MvvmScarletToolkit.Wpf.Features.FileSystemBrowser
{
    public sealed record ScarletFileInfo
    {
        public ScarletFileInfo(FileInfo info)
        {
            FullName = info.FullName;
            Name = info.Name;
            Exists = info.Exists;
            IsHidden = (info.Attributes & FileAttributes.Hidden) != 0;
            CreationTimeUtc = info.CreationTimeUtc;
            LastAccessTimeUtc = info.LastAccessTimeUtc;
            LastWriteTimeUtc = info.LastWriteTimeUtc;
            IsAccessProhibited = false;
        }

        public ScarletFileInfo()
        {
            Name = string.Empty;
            FullName = string.Empty;
        }

        public string FullName { get; init; }
        public string Name { get; init; }
        public bool Exists { get; init; }
        public bool IsHidden { get; init; }
        public DateTime? CreationTimeUtc { get; init; }
        public DateTime? LastAccessTimeUtc { get; init; }
        public DateTime? LastWriteTimeUtc { get; init; }
        public bool IsAccessProhibited { get; init; }
    }
}
