using CommunityToolkit.Mvvm.Messaging;
using MvvmScarletToolkit.Wpf.Features.FileSystemBrowser.Directories;
using MvvmScarletToolkit.Wpf.Features.FileSystemBrowser.Drives;
using MvvmScarletToolkit.Wpf.Features.FileSystemBrowser.Files;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

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

        Task<bool> IsEmpty(IFileSystemParent parent, CancellationToken token);

        Task<ScarletFileInfo?> GetFileInfo(string filePath, CancellationToken token);

        Task<ScarletDriveInfo?> GetDriveInfo(string filePath, CancellationToken token);

        Task<ScarletDirectoryInfo?> GetDirectoryInfo(string filePath, CancellationToken token);
    }
}
