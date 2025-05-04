using MvvmScarletToolkit.Wpf.Features.FileSystemBrowser;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit
{
    public interface IFileSystemViewModelFactory
    {
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

        Task<bool> IsEmpty(IFileSystemParent parent, CancellationToken token);

        Task<ScarletFileInfo?> GetFileInfo(string filePath, CancellationToken token);

        Task<ScarletDriveInfo?> GetDriveInfo(string filePath, CancellationToken token);

        Task<ScarletDirectoryInfo?> GetDirectoryInfo(string filePath, CancellationToken token);
    }
}
