using MvvmScarletToolkit.Wpf.Features.FileSystemBrowser.Directories;
using MvvmScarletToolkit.Wpf.Features.FileSystemBrowser.Interfaces;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Wpf.Features.FileSystemBrowser
{
    public static class FileSystemViewModelFactoryExtensions
    {
        public static async Task<IReadOnlyCollection<IFileSystemChild>> GetChildren(this IFileSystemViewModelFactory factory, IFileSystemDirectory parent, IReadOnlyCollection<FileAttributes> fileAttributes, IReadOnlyCollection<FileAttributes> folderAttributes, CancellationToken token)
        {
            return await factory.GetChildren((IFileSystemParent)parent, fileAttributes, folderAttributes, token);
        }

        public static async Task<IReadOnlyCollection<IFileSystemChild>> GetChildren(this IFileSystemViewModelFactory factory, IFileSystemParent parent, IReadOnlyCollection<FileAttributes> fileAttributes, IReadOnlyCollection<FileAttributes> folderAttributes, CancellationToken token)
        {
            if (await factory.IsEmpty(parent, token))
            {
                return Enumerable.Empty<IFileSystemChild>().ToList();
            }

            var directories = factory.GetDirectories(parent, fileAttributes, folderAttributes, token);
            var files = factory.GetFiles(parent, fileAttributes, token);

            await Task.WhenAll(directories, files);

            var results = new List<IFileSystemChild>(directories.Result.Count + files.Result.Count);
            results.AddRange(directories.Result);
            results.AddRange(files.Result);

            return results;
        }
    }
}
