using System.IO;

namespace MvvmScarletToolkit.Wpf.Features.FileSystemBrowser.Interfaces
{
    public interface IFileSystemChild : IFileSystemInfo
    {
        DateTime? LastWriteTimeUtc { get; }
        DateTime? LastAccessTimeUtc { get; }
        DateTime? CreationTimeUtc { get; }
        bool IsHidden { get; }
        IFileSystemParent? Parent { get; }

        /// <summary>
        /// The size in bytes, <see langword="null"/> for folders.
        /// </summary>
        long? Length { get; }

        FileAttributes Attributes { get; }

        /// <summary>
        /// The path of the containing folder.
        /// </summary>
        string Location { get; }

        string TypeName { get; }
    }
}
