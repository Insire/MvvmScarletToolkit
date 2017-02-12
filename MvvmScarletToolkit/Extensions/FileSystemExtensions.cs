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
        public static List<IFileSystemInfo> GetChildren(this DirectoryInfo directory)
        {
            var result = new List<IFileSystemInfo>();

            if (!CanAccess(directory.FullName) && directory.DirectoryIsEmpty())
                return result;

            result.AddRange(GetDirectories(directory.FullName));
            result.AddRange(GetFiles(directory.FullName));

            return result;
        }

        public static List<IFileSystemInfo> GetChildren(this IFileSystemDirectory directory)
        {
            var result = new List<IFileSystemInfo>();

            if (!CanAccess(directory.FullName) && directory.DirectoryIsEmpty())
                return result;

            result.AddRange(GetDirectories(directory.FullName));
            result.AddRange(GetFiles(directory.FullName));

            return result;
        }

        private static bool CanAccess(string path)
        {
            var permission = new FileIOPermission(FileIOPermissionAccess.AllAccess, AccessControlActions.View, path);

            try
            {
                permission.Demand();
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
        }

        private static List<ScarletDirectory>GetDirectories(string path)
        {
            var result = new List<ScarletDirectory>();
            try
            {
                var directories = Directory.GetDirectories(path)
                                            .Select(p => new DirectoryInfo(p))
                                            .Select(p => new ScarletDirectory(p))
                                            .ToList();

                result.AddRange(directories);
            }
            catch(UnauthorizedAccessException)
            {
                Debug.WriteLine($"{nameof(UnauthorizedAccessException)} occured during reading off {path}");
            }
            return result;
        }

        private static List<ScarletFile> GetFiles(string path)
        {
            var result = new List<ScarletFile>();
            try
            {
                var files = Directory.GetFiles(path)
                                        .Select(p => new FileInfo(p))
                                        .Select(p => new ScarletFile(p))
                                                    .ToList();

                result.AddRange(files);
            }
            catch (UnauthorizedAccessException)
            {
                Debug.WriteLine($"{nameof(UnauthorizedAccessException)} occured during reading off {path}");
            }
            return result;
        }

        public static bool DirectoryIsEmpty(this IFileSystemDirectory info)
        {
            if (!Directory.Exists(info.FullName))
                return false;

            return DirectoryIsEmpty(info.FullName);
        }

        public static bool DirectoryIsEmpty(this DirectoryInfo info)
        {
            if (!Directory.Exists(info.FullName))
                return false;

            return DirectoryIsEmpty(info.FullName);
        }

        public static bool DirectoryIsEmpty(string path)
        {
            return !Directory.EnumerateFileSystemEntries(path).Any();
        }
    }
}
