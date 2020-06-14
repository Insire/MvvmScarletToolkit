using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MvvmScarletToolkit
{
    public static class FileSystemViewModelFactoryExtensions
    {
        public static async Task<IReadOnlyCollection<IFileSystemChild>> GetChildren(this IFileSystemViewModelFactory factory, IFileSystemDirectory parent, FileAttributes fileAttributes, FileAttributes folderAttributes)
        {
            var canAccess = await factory.CanAccess(parent);
            if (!canAccess)
            {
                return Enumerable.Empty<IFileSystemChild>().ToList();
            }

            return await factory.GetChildren((IFileSystemParent)parent, fileAttributes, folderAttributes);
        }

        public static async Task<IReadOnlyCollection<IFileSystemChild>> GetChildren(this IFileSystemViewModelFactory factory, IFileSystemParent parent, FileAttributes fileAttributes, FileAttributes folderAttributes)
        {
            if (await factory.IsEmpty(parent))
            {
                return Enumerable.Empty<IFileSystemChild>().ToList();
            }

            var directories = factory.GetDirectories(parent, fileAttributes, folderAttributes);
            var files = factory.GetFiles(parent, fileAttributes);

            await Task.WhenAll(directories, files);

            var results = new List<IFileSystemChild>(directories.Result.Count + files.Result.Count);
            results.AddRange(directories.Result);
            results.AddRange(files.Result);

            return results;
        }
    }
}
