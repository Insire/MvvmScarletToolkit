using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MvvmScarletToolkit
{
    public static class FileSystemViewModelFactoryExtensions
    {
        public static async Task<IReadOnlyCollection<IFileSystemChild>> GetChildren(this IFileSystemViewModelFactory factory, IFileSystemDirectory parent, IReadOnlyCollection<FileAttributes> fileAttributes, IReadOnlyCollection<FileAttributes> folderAttributes)
        {
            return await factory.GetChildren((IFileSystemParent)parent, fileAttributes, folderAttributes);
        }

        public static async Task<IReadOnlyCollection<IFileSystemChild>> GetChildren(this IFileSystemViewModelFactory factory, IFileSystemParent parent, IReadOnlyCollection<FileAttributes> fileAttributes, IReadOnlyCollection<FileAttributes> folderAttributes)
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

        internal static bool IFileSystemChildComparer(this IFileSystemChild current, IFileSystemChild other)
        {
            return current.FullName == other.FullName && current.Exists == other.Exists;
        }

        internal static IFileSystemChild IFileSystemChildMapper(this IFileSystemChild child)
        {
            return child;
        }
    }
}
