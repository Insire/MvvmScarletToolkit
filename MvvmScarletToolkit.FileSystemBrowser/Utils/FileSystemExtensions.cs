using MvvmScarletToolkit.Abstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Permissions;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.FileSystemBrowser
{
    public static class FileSystemExtensions
    {
        public static IEnumerable<IFileSystemInfo> GetChildren(this ScarletFileSystemContainerBase directory, IDepth depth, IScarletDispatcher dispatcher)
        {
            return !CanAccess(directory.FullName) && directory.DirectoryIsEmpty()
                ? Enumerable.Empty<IFileSystemInfo>()
                : GetDirectories(directory.FullName, depth, directory, dispatcher)
                    .Concat(GetFiles(directory.FullName, depth, directory));
        }

        private static bool CanAccess(string path)
        {
            var permission = new FileIOPermission(FileIOPermissionAccess.AllAccess, AccessControlActions.View, path);

            try
            {
                permission.Demand();
                return true;
            }
            catch (UnauthorizedAccessException ex)
            {
                Debug.WriteLine($"{nameof(UnauthorizedAccessException)} occured during reading off {path}");
                Debug.WriteLine(ex.Message);

                return false;
            }
        }

        private static IEnumerable<IFileSystemInfo> GetDirectories(string path, IDepth depth, IFileSystemDirectory parent, IScarletDispatcher dispatcher)
        {
            try
            {
                return Directory.GetDirectories(path)
                                            .Select(p => new DirectoryInfo(p))
                                            .Where(p => (p.Attributes & FileAttributes.Directory) != 0
                                                        && (p.Attributes & FileAttributes.Hidden) == 0
                                                        && (p.Attributes & FileAttributes.System) == 0
                                                        && (p.Attributes & FileAttributes.Offline) == 0
                                                        && (p.Attributes & FileAttributes.Encrypted) == 0)
                                            .Select(p => new ScarletDirectory(p, depth, parent, dispatcher))
                                            .ToArray();
            }
            catch (UnauthorizedAccessException ex)
            {
                Debug.WriteLine($"{nameof(UnauthorizedAccessException)} occured during reading off {path}");
                Debug.WriteLine(ex.Message);
            }

            return Enumerable.Empty<IFileSystemInfo>();
        }

        private static IEnumerable<IFileSystemInfo> GetFiles(string path, IDepth depth, IFileSystemDirectory parent)
        {
            try
            {
                return Directory.GetFiles(path)
                        .Select(p => new FileInfo(p))
                        .Where(p => (p.Attributes & FileAttributes.Directory) == 0
                                    && (p.Attributes & FileAttributes.Hidden) == 0
                                    && (p.Attributes & FileAttributes.System) == 0
                                    && (p.Attributes & FileAttributes.Offline) == 0
                                    && (p.Attributes & FileAttributes.Encrypted) == 0)
                        .Select(p => new ScarletFile(p, depth, parent))
                        .ToArray();
            }
            catch (UnauthorizedAccessException ex)
            {
                Debug.WriteLine($"{nameof(UnauthorizedAccessException)} occured during reading off {path}");
                Debug.WriteLine(ex.Message);
            }

            return Enumerable.Empty<IFileSystemInfo>();
        }

        public static bool DirectoryIsEmpty(this ScarletFileSystemContainerBase info)
        {
            return !Directory.Exists(info.FullName)
                ? false
                : DirectoryIsEmpty(info.FullName);
        }

        public static bool DirectoryIsEmpty(this string path)
        {
            return !Directory.EnumerateFileSystemEntries(path).Any();
        }

        public static async Task ExpandPath(this IFileSystemInfo item)
        {
            if (!item.IsExpanded)
            {
                await item.ToggleExpandCommand.ExecuteAsync(null).ConfigureAwait(false);
            }

            var parent = item.Parent;

            while (!(parent is null))
            {
                if (!parent.IsExpanded)
                {
                    await parent.ToggleExpandCommand.ExecuteAsync(null).ConfigureAwait(false);
                }

                parent = parent.Parent;
            }
        }
    }
}
