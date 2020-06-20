using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace MvvmScarletToolkit
{
    public interface IFileSystemViewModelFactory
    {
        Task<IReadOnlyCollection<IFileSystemDrive>> GetDrives(IReadOnlyCollection<DriveType> types, IReadOnlyCollection<FileAttributes> fileAttributes, IReadOnlyCollection<FileAttributes> folderAttributes);

        Task<IReadOnlyCollection<IFileSystemDirectory>> GetDirectories(IFileSystemParent parent, IReadOnlyCollection<FileAttributes> fileAttributes, IReadOnlyCollection<FileAttributes> folderAttributes);

        Task<IReadOnlyCollection<IFileSystemFile>> GetFiles(IFileSystemParent parent, IReadOnlyCollection<FileAttributes> fileAttributes);

        Task<bool> IsEmpty(IFileSystemParent parent);

        Task<bool> CanAccess(IFileSystemChild child);
    }
}
