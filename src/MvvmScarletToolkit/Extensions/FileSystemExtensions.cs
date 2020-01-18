using MvvmScarletToolkit.FileSystemBrowser;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Permissions;

namespace MvvmScarletToolkit
{
    public static class FileSystemExtensions
    {
        public static IEnumerable<IFileSystemInfo> GetChildren(this ScarletFileSystemContainerBase directory, ICommandBuilder commandBuilder)
        {
            return !CanAccess(directory.FullName) && directory.DirectoryIsEmpty()
                ? Enumerable.Empty<IFileSystemInfo>()
                : GetDirectories(directory.FullName, directory, commandBuilder)
                    .Concat(GetFiles(directory.FullName, directory, commandBuilder));
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

        private static IEnumerable<IFileSystemInfo> GetDirectories(string path, IFileSystemDirectory parent, ICommandBuilder commandBuilder)
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
                                            .Select(p => new ScarletDirectory(p, parent, commandBuilder));
            }
            catch (UnauthorizedAccessException ex)
            {
                Debug.WriteLine($"{nameof(UnauthorizedAccessException)} occured during reading off {path}");
                Debug.WriteLine(ex.Message);
            }

            return Enumerable.Empty<IFileSystemInfo>();
        }

        private static IEnumerable<IFileSystemInfo> GetFiles(string path, IFileSystemDirectory parent, ICommandBuilder commandBuilder)
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
                        .Select(p => new ScarletFile(p, parent, commandBuilder));
            }
            catch (UnauthorizedAccessException ex)
            {
                Debug.WriteLine($"{nameof(UnauthorizedAccessException)} occured during reading off {path}");
                Debug.WriteLine(ex.Message);
            }

            return Enumerable.Empty<IFileSystemInfo>();
        }

        private static bool DirectoryIsEmpty(this ScarletFileSystemContainerBase info)
        {
            return !Directory.Exists(info.FullName)
                ? false
                : !Directory.EnumerateFileSystemEntries(info.FullName).Any();
        }

        public static void ExpandPath(this IFileSystemInfo item)
        {
            if (!item.IsExpanded)
            {
                item.ToggleExpandCommand.Execute(null);
            }

            var parent = item.Parent;

            while (!(parent is null))
            {
                if (!parent.IsExpanded)
                {
                    parent.ToggleExpandCommand.Execute(null);
                }

                parent = parent.Parent;
            }
        }
    }
}
