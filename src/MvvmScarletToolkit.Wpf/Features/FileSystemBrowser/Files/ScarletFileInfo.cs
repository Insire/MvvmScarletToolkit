using System;
using System.IO;

namespace MvvmScarletToolkit.Wpf.Features.FileSystemBrowser.Files
{
    public sealed record ScarletFileInfo
    {
        public ScarletFileInfo(FileInfo info, string mimeType)
        {
            FullName = info.FullName;
            Name = info.Name;
            Extension = info.Extension;
            Exists = info.Exists;
            IsHidden = (info.Attributes & FileAttributes.Hidden) != 0;
            CreationTimeUtc = info.CreationTimeUtc;
            LastAccessTimeUtc = info.LastAccessTimeUtc;
            LastWriteTimeUtc = info.LastWriteTimeUtc;
            IsAccessProhibited = false;
            MimeType = mimeType;
            IsImage = mimeType.StartsWith("image", StringComparison.InvariantCultureIgnoreCase);
        }

        public ScarletFileInfo()
        {
            Name = string.Empty;
            FullName = string.Empty;
            Extension = string.Empty;
            MimeType = string.Empty;
        }

        public string Name { get; init; }
        public string FullName { get; init; }
        public string Extension { get; init; }
        public string MimeType { get; init; }
        public bool Exists { get; init; }
        public bool IsHidden { get; init; }
        public bool IsImage { get; init; }
        public DateTime? CreationTimeUtc { get; init; }
        public DateTime? LastAccessTimeUtc { get; init; }
        public DateTime? LastWriteTimeUtc { get; init; }
        public bool IsAccessProhibited { get; init; }
    }
}
