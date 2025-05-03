using System;
using System.IO;

namespace MvvmScarletToolkit.Wpf.Features.FileSystemBrowser
{
    public sealed record ScarletDirectoryInfo
    {
        public ScarletDirectoryInfo(DirectoryInfo info)
        {
            Name = info.Name;
            FullName = info.FullName;
            Exists = info.Exists;
            IsHidden = (info.Attributes & FileAttributes.Hidden) != 0;
            CreationTimeUtc = info.CreationTimeUtc;
            LastAccessTimeUtc = info.LastAccessTimeUtc;
            LastWriteTimeUtc = info.LastWriteTimeUtc;
        }

        public string Name { get; init; }
        public string FullName { get; init; }
        public bool Exists { get; init; }
        public bool IsHidden { get; init; }
        public DateTime? CreationTimeUtc { get; init; }
        public DateTime? LastAccessTimeUtc { get; init; }
        public DateTime? LastWriteTimeUtc { get; init; }
    }
}
