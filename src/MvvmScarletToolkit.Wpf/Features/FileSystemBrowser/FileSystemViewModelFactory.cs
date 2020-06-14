using MvvmScarletToolkit.Wpf.FileSystemBrowser;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Permissions;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Wpf.Features.FileSystemBrowser
{
    public sealed class FileSystemViewModelFactory : IFileSystemViewModelFactory
    {
        private readonly IScarletCommandBuilder _commandBuilder;

        public FileSystemViewModelFactory(IScarletCommandBuilder commandBuilder)
        {
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
        }

        public Task<bool> IsEmpty(IFileSystemParent parent)
        {
            return parent switch
            {
                IFileSystemDrive drive => Task.Run(() => IsDriveEmpty(drive)),
                IFileSystemDirectory directory => Task.Run(() => IsDirectoryEmpty(directory)),
                _ => throw new NotImplementedException(),
            };
        }

        private static bool IsDriveEmpty(IFileSystemDrive drive)
        {
            if (drive.DriveType == DriveType.NoRootDirectory || drive.DriveType == DriveType.Unknown)
            {
                return true;
            }

            //var driveInfo = new DriveInfo(drive.FullName);
            return IsDirectoryEmpty(drive.FullName);
        }

        private static bool IsDirectoryEmpty(IFileSystemDirectory directory)
        {
            return IsDirectoryEmpty(directory.FullName);
        }

        private static bool IsDirectoryEmpty(string directoryPath)
        {
            return Directory.Exists(directoryPath) && !Directory.EnumerateFileSystemEntries(directoryPath).Any();
        }

        public async Task<bool> CanAccess(IFileSystemChild child)
        {
            var can = await Task.Run(() => CanAccess(child.FullName));

            return can;
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

        public async Task<IReadOnlyCollection<IFileSystemDrive>> GetDrives(IReadOnlyCollection<DriveType> types, FileAttributes fileAttributes, FileAttributes folderAttributes)
        {
            var drives = await Task.Run(() => GetDrivesInternal(types, fileAttributes, folderAttributes).ToList());

            return drives;
        }

        private IEnumerable<IFileSystemDrive> GetDrivesInternal(IReadOnlyCollection<DriveType> types, FileAttributes fileAttributes, FileAttributes folderAttributes)
        {
            if (types.Count == 0)
            {
                return Enumerable.Empty<IFileSystemDrive>();
            }

            var drives = DriveInfo
                 .GetDrives();

            if (drives.Length == 0)
            {
                return Enumerable.Empty<IFileSystemDrive>();
            }

            return drives
                .Where(p => p.IsReady && types.Any(q => q == p.DriveType))
                .Select(p => new ScarletDrive(p, _commandBuilder, this, fileAttributes, folderAttributes));
        }

        public async Task<IReadOnlyCollection<IFileSystemDirectory>> GetDirectories(IFileSystemParent parent, FileAttributes fileAttributes, FileAttributes folderAttributes)
        {
            var directories = await Task.Run(() => GetDirectoriesInternal(parent, fileAttributes, folderAttributes).ToList());

            return directories;
        }

        private IEnumerable<IFileSystemDirectory> GetDirectoriesInternal(IFileSystemParent parent, FileAttributes fileAttributes, FileAttributes folderAttributes)
        {
            try
            {
                var directories = Directory.GetDirectories(parent.FullName);

                return directories
                            .Select(p => new DirectoryInfo(p))
                            .Where(p => p.Attributes.HasFlag(FileAttributes.Directory))
                            .Select(p => new ScarletDirectory(p, fileAttributes, folderAttributes, parent, _commandBuilder, this));
            }
            catch (UnauthorizedAccessException ex)
            {
                Debug.WriteLine($"{nameof(UnauthorizedAccessException)} occured during reading off {parent.FullName}");
                Debug.WriteLine(ex.Message);
            }

            return Enumerable.Empty<IFileSystemDirectory>();
        }

        public async Task<IReadOnlyCollection<IFileSystemFile>> GetFiles(IFileSystemParent parent, FileAttributes fileAttributes)
        {
            var files = await Task.Run(() => GetFilesInternal(parent, fileAttributes).ToList());

            return files;
        }

        private IEnumerable<IFileSystemFile> GetFilesInternal(IFileSystemParent parent, FileAttributes fileAttributes)
        {
            try
            {
                return Directory.GetFiles(parent.FullName)
                        .Select(p => new FileInfo(p))
                        .Where(p => p.Attributes.HasFlag(fileAttributes))
                        .Select(p => new ScarletFile(p, parent, _commandBuilder));
            }
            catch (UnauthorizedAccessException ex)
            {
                Debug.WriteLine($"{nameof(UnauthorizedAccessException)} occured during reading off {parent.FullName}");
                Debug.WriteLine(ex.Message);
            }

            return Enumerable.Empty<IFileSystemFile>();
        }
    }
}
