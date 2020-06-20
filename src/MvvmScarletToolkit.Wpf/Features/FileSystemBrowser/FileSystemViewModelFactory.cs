using MvvmScarletToolkit.Wpf.FileSystemBrowser;
using System;
using System.Collections.Concurrent;
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
        private readonly ConcurrentDictionary<string, string> _noAccessLookup;

        public FileSystemViewModelFactory(IScarletCommandBuilder commandBuilder)
        {
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
            _noAccessLookup = new ConcurrentDictionary<string, string>();
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

            return false;
        }

        private bool IsDirectoryEmpty(IFileSystemDirectory directory)
        {
            return IsDirectoryEmpty(directory.FullName);
        }

        private bool IsDirectoryEmpty(string directoryPath)
        {
            var result = true;
            if (_noAccessLookup.ContainsKey(directoryPath))
            {
                return result;
            }

            try
            {
                if (Directory.Exists(directoryPath))
                {
                    var query = Directory.EnumerateFileSystemEntries(directoryPath);

                    result = !query.Any();
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                _noAccessLookup.TryAdd(directoryPath, directoryPath);

                Debug.WriteLine(ex.Message);
            }

            return result;
        }

        public async Task<bool> CanAccess(IFileSystemChild child)
        {
            var can = await Task.Run(() => CanAccess(child.FullName));

            return can;
        }

        private bool CanAccess(string path)
        {
            if (_noAccessLookup.ContainsKey(path))
            {
                return false;
            }

            var permission = new FileIOPermission(FileIOPermissionAccess.Read, AccessControlActions.View, path);

            try
            {
                permission.Demand();
                return true;
            }
            catch (UnauthorizedAccessException ex)
            {
                _noAccessLookup.TryAdd(path, path);

                Debug.WriteLine(ex.Message);

                return false;
            }
        }

        public async Task<IReadOnlyCollection<IFileSystemDrive>> GetDrives(IReadOnlyCollection<DriveType> types, IReadOnlyCollection<FileAttributes> fileAttributes, IReadOnlyCollection<FileAttributes> folderAttributes)
        {
            var drives = await Task.Run(() => GetDrivesInternal(types, fileAttributes, folderAttributes).ToList());

            return drives;
        }

        private IEnumerable<IFileSystemDrive> GetDrivesInternal(IReadOnlyCollection<DriveType> types, IReadOnlyCollection<FileAttributes> fileAttributes, IReadOnlyCollection<FileAttributes> folderAttributes)
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

        public async Task<IReadOnlyCollection<IFileSystemDirectory>> GetDirectories(IFileSystemParent parent, IReadOnlyCollection<FileAttributes> fileAttributes, IReadOnlyCollection<FileAttributes> folderAttributes)
        {
            var directories = await Task.Run(() => GetDirectoriesInternal(parent, fileAttributes, folderAttributes).ToList());

            return directories;
        }

        private IEnumerable<IFileSystemDirectory> GetDirectoriesInternal(IFileSystemParent parent, IReadOnlyCollection<FileAttributes> fileAttributes, IReadOnlyCollection<FileAttributes> folderAttributes)
        {
            if (_noAccessLookup.ContainsKey(parent.FullName))
            {
                return Enumerable.Empty<IFileSystemDirectory>();
            }

            try
            {
                var directoryInfos = Directory
                    .GetDirectories(parent.FullName)
                    .Select(p => new DirectoryInfo(p));

                return directoryInfos
                    .Where(p => folderAttributes.Any(q => p.Attributes.HasFlag(q)))
                    .Select(p => new ScarletDirectory(p, fileAttributes, folderAttributes, parent, _commandBuilder, this));
            }
            catch (UnauthorizedAccessException ex)
            {
                Debug.WriteLine(ex.Message);

                _noAccessLookup.TryAdd(parent.FullName, parent.FullName);
            }

            return Enumerable.Empty<IFileSystemDirectory>();
        }

        public async Task<IReadOnlyCollection<IFileSystemFile>> GetFiles(IFileSystemParent parent, IReadOnlyCollection<FileAttributes> fileAttributes)
        {
            var files = await Task.Run(() => GetFilesInternal(parent, fileAttributes).ToList());

            return files;
        }

        private IEnumerable<IFileSystemFile> GetFilesInternal(IFileSystemParent parent, IReadOnlyCollection<FileAttributes> fileAttributes)
        {
            if (_noAccessLookup.ContainsKey(parent.FullName))
            {
                return Enumerable.Empty<IFileSystemFile>();
            }

            try
            {
                var fileInfos = Directory
                    .GetFiles(parent.FullName)
                    .Select(p => new FileInfo(p));

                return fileInfos
                    .Where(p => fileAttributes.Any(q => p.Attributes.HasFlag(q)))
                    .Select(p => new ScarletFile(p, parent, _commandBuilder));
            }
            catch (UnauthorizedAccessException ex)
            {
                Debug.WriteLine(ex.Message);

                _noAccessLookup.TryAdd(parent.FullName, parent.FullName);
            }

            return Enumerable.Empty<IFileSystemFile>();
        }
    }
}
