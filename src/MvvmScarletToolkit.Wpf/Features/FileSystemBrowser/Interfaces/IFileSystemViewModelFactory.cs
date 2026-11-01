using CommunityToolkit.Mvvm.Messaging;
using MvvmScarletToolkit.Wpf.Features.FileSystemBrowser.Directories;
using MvvmScarletToolkit.Wpf.Features.FileSystemBrowser.Drives;
using MvvmScarletToolkit.Wpf.Features.FileSystemBrowser.Files;
using System.IO;

namespace MvvmScarletToolkit.Wpf.Features.FileSystemBrowser.Interfaces
{
    public interface IFileSystemViewModelFactory
    {
        IMessenger Messenger { get; }

        Task<IReadOnlyCollection<IFileSystemDrive>> GetDrives(
            IReadOnlyCollection<DriveType> types,
            IReadOnlyCollection<FileAttributes> fileAttributes,
            IReadOnlyCollection<FileAttributes> folderAttributes,
            CancellationToken token);

        Task<IReadOnlyCollection<IFileSystemDirectory>> GetDirectories(
            IFileSystemParent parent,
            IReadOnlyCollection<FileAttributes> fileAttributes,
            IReadOnlyCollection<FileAttributes> folderAttributes,
            CancellationToken token);

        Task<IReadOnlyCollection<IFileSystemFile>> GetFiles(
            IFileSystemParent parent,
            IReadOnlyCollection<FileAttributes> fileAttributes,
            CancellationToken token);

        Task<bool?> IsEmpty(IFileSystemParent parent, CancellationToken token);

        Task<ScarletFileInfo?> GetFileInfo(string filePath, CancellationToken token);

        Task<ScarletDriveInfo?> GetDriveInfo(string filePath, CancellationToken token);

        Task<ScarletDirectoryInfo?> GetDirectoryInfo(string filePath, CancellationToken token);

        /// <summary>
        /// Reads the header of a file, to back up the type expected from its extension.
        /// </summary>
        Task<ScarletFileHeader> GetFileHeader(string filePath, CancellationToken token);

        /// <summary>
        /// Returns the owner and access rules, or <see langword="null"/> if they can't be read.
        /// </summary>
        Task<ScarletPermissions?> GetPermissions(string path, bool isDirectory, CancellationToken token);

        /// <summary>
        /// Searches a folder and its subfolders by name, yielding the matches per searched directory.
        /// </summary>
        IAsyncEnumerable<FileSystemSearchBatch> Search(
            IFileSystemParent parent,
            string query,
            IReadOnlyCollection<FileAttributes> fileAttributes,
            IReadOnlyCollection<FileAttributes> folderAttributes,
            CancellationToken token);
    }
}
