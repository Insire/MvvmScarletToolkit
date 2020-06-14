using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace MvvmScarletToolkit
{
    public interface IFileSystemViewModelFactory
    {
        Task<IReadOnlyCollection<IFileSystemDrive>> GetDrives(IReadOnlyCollection<DriveType> types, FileAttributes fileAttributes, FileAttributes folderAttributes);

        Task<IReadOnlyCollection<IFileSystemDirectory>> GetDirectories(IFileSystemParent parent, FileAttributes fileAttributes, FileAttributes folderAttributes);

        Task<IReadOnlyCollection<IFileSystemFile>> GetFiles(IFileSystemParent parent, FileAttributes fileAttributes);

        Task<bool> IsEmpty(IFileSystemParent parent);

        Task<bool> CanAccess(IFileSystemChild child);
    }
}
