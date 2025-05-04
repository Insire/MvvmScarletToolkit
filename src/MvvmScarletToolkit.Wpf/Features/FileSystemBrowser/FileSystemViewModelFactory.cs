using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using MvvmScarletToolkit.Wpf.FileSystemBrowser;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Wpf.Features.FileSystemBrowser
{
    public sealed class FileSystemViewModelFactory : IFileSystemViewModelFactory
    {
        private readonly ILogger<FileSystemViewModelFactory> _logger;
        private readonly IScheduler _scheduler;
        private readonly ConcurrentDictionary<string, string> _noAccessLookup;
        private readonly IReadOnlyList<IMimeTypeResolver> _mimetypeResolvers;

        public IMessenger Messenger { get; }

        public FileSystemViewModelFactory(ILogger<FileSystemViewModelFactory> logger, IScheduler scheduler)
        {
            _logger = logger;
            _scheduler = scheduler;
            Messenger = new WeakReferenceMessenger();
            _noAccessLookup = new ConcurrentDictionary<string, string>();
            _mimetypeResolvers = new List<IMimeTypeResolver>()
            {
                new MagicNumberMimeTypeResolver(),
                new RegistryFileExtensionMimeTypeResolver(),
                new UrlMonMimeTypeResolver(),
                new StaticFileExtensionMimeTypeResolver(),
            };
        }

        public async Task<ScarletFileInfo?> GetFileInfo(string filePath, CancellationToken token)
        {
            try
            {
                var info = await Task.Run(() => new FileInfo(filePath), token);
                var mimeType = await Task.Run(() => GetMimeType(info), token);

                return new ScarletFileInfo(info, mimeType ?? string.Empty);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Failed to get file info");

                return new ScarletFileInfo()
                {
                    CreationTimeUtc = null,
                    Exists = true,
                    FullName = filePath,
                    IsHidden = false,
                    LastAccessTimeUtc = null,
                    LastWriteTimeUtc = null,
                    Name = Path.GetFileName(filePath),
                    IsAccessProhibited = true,
                };
            }
            catch (FileNotFoundException ex)
            {
                _logger.LogError(ex, "Failed to get file info");

                return new ScarletFileInfo()
                {
                    CreationTimeUtc = null,
                    Exists = false,
                    FullName = filePath,
                    IsHidden = false,
                    LastAccessTimeUtc = null,
                    LastWriteTimeUtc = null,
                    Name = Path.GetFileName(filePath),
                    IsAccessProhibited = true,
                };
            }
            catch (IOException ex)
            {
                _logger.LogError(ex, "Failed to get file info");

                return new ScarletFileInfo()
                {
                    CreationTimeUtc = null,
                    Exists = false,
                    FullName = filePath,
                    IsHidden = false,
                    LastAccessTimeUtc = null,
                    LastWriteTimeUtc = null,
                    Name = Path.GetFileName(filePath),
                    IsAccessProhibited = true,
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get file info");

                return null;
            }
        }

        public async Task<ScarletDirectoryInfo?> GetDirectoryInfo(string filePath, CancellationToken token)
        {
            try
            {
                var info = await Task.Run(() => new DirectoryInfo(filePath), token);

                return new ScarletDirectoryInfo(info);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Failed to get directory info");

                return new ScarletDirectoryInfo()
                {
                    CreationTimeUtc = null,
                    Exists = true,
                    FullName = filePath,
                    IsHidden = false,
                    LastAccessTimeUtc = null,
                    LastWriteTimeUtc = null,
                    Name = Path.GetFileName(filePath),
                    IsAccessProhibited = true,
                };
            }
            catch (DirectoryNotFoundException ex)
            {
                _logger.LogError(ex, "Failed to get directory info");

                return new ScarletDirectoryInfo()
                {
                    CreationTimeUtc = null,
                    Exists = false,
                    FullName = filePath,
                    IsHidden = false,
                    LastAccessTimeUtc = null,
                    LastWriteTimeUtc = null,
                    Name = Path.GetFileName(filePath),
                    IsAccessProhibited = true,
                };
            }
            catch (IOException ex)
            {
                _logger.LogError(ex, "Failed to get directory info");

                return new ScarletDirectoryInfo()
                {
                    CreationTimeUtc = null,
                    Exists = true,
                    FullName = filePath,
                    IsHidden = false,
                    LastAccessTimeUtc = null,
                    LastWriteTimeUtc = null,
                    Name = Path.GetFileName(filePath),
                    IsAccessProhibited = true,
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get directory info");
            }

            return null;
        }

        public async Task<ScarletDriveInfo?> GetDriveInfo(string filePath, CancellationToken token)
        {
            try
            {
                var info = await Task.Run(() => new DriveInfo(filePath), token);
                return new ScarletDriveInfo(info);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Failed to get drive info");
                return new ScarletDriveInfo()
                {
                    AvailableFreeSpace = 0,
                    DriveFormat = null,
                    DriveType = DriveType.Unknown,
                    IsReady = false,
                    TotalFreeSpace = 0,
                    TotalSize = 0,
                    FullName = filePath,
                    Name = Path.GetFileName(filePath),
                    IsAccessProhibited = true,
                };
            }
            catch (DriveNotFoundException ex)
            {
                _logger.LogError(ex, "Failed to get drive info");
            }
            catch (IOException ex)
            {
                _logger.LogError(ex, "Failed to get drive info");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get drive info");
            }

            return null;
        }

        public Task<bool> IsEmpty(IFileSystemParent parent, CancellationToken token)
        {
            return parent switch
            {
                IFileSystemDrive drive => Task.Run(() => IsDriveEmpty(drive), token),
                IFileSystemDirectory directory => Task.Run(() => IsDirectoryEmpty(directory), token),
                _ => throw new NotImplementedException(),
            };
        }

        private string? GetMimeType(FileInfo fileInfo)
        {
            foreach (var mimeTypeResolver in _mimetypeResolvers)
            {
                var mimeType = string.Empty;

                try
                {
                    mimeType = mimeTypeResolver.Get(fileInfo);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to get mimetype from file");
                    continue;
                }

                if (!string.IsNullOrEmpty(mimeType))
                {
                    return mimeType;
                }
            }

            return null;
        }

        private static bool IsDriveEmpty(IFileSystemDrive drive)
        {
            return drive.DriveType == DriveType.NoRootDirectory || drive.DriveType == DriveType.Unknown;
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

                _logger.LogError(ex, "Failed to get directory info");
            }

            return result;
        }

        public Task<IReadOnlyCollection<IFileSystemDrive>> GetDrives(
            IReadOnlyCollection<DriveType> types,
            IReadOnlyCollection<FileAttributes> fileAttributes,
            IReadOnlyCollection<FileAttributes> folderAttributes,
            CancellationToken token)
        {
            return Task.Run<IReadOnlyCollection<IFileSystemDrive>>(() => GetDrivesInternal(types, fileAttributes, folderAttributes).ToList(), token);
        }

        private IEnumerable<IFileSystemDrive> GetDrivesInternal(
            IReadOnlyCollection<DriveType> types,
            IReadOnlyCollection<FileAttributes> fileAttributes,
            IReadOnlyCollection<FileAttributes> folderAttributes)
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
                .Select(p => new ScarletDrive(_scheduler, new ScarletDriveInfo(p), this, fileAttributes, folderAttributes));
        }

        public Task<IReadOnlyCollection<IFileSystemDirectory>> GetDirectories(
            IFileSystemParent parent,
            IReadOnlyCollection<FileAttributes> fileAttributes,
            IReadOnlyCollection<FileAttributes> folderAttributes,
            CancellationToken token)
        {
            return Task.Run<IReadOnlyCollection<IFileSystemDirectory>>(() => GetDirectoriesInternal(parent, fileAttributes, folderAttributes).ToList(), token);
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
                    .Select(p => new ScarletDirectory(_scheduler, new ScarletDirectoryInfo(p), fileAttributes, folderAttributes, parent, this));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Failed to get directory info");

                _noAccessLookup.TryAdd(parent.FullName, parent.FullName);
            }

            return Enumerable.Empty<IFileSystemDirectory>();
        }

        public Task<IReadOnlyCollection<IFileSystemFile>> GetFiles(IFileSystemParent parent, IReadOnlyCollection<FileAttributes> fileAttributes, CancellationToken token)
        {
            return Task.Run<IReadOnlyCollection<IFileSystemFile>>(() => GetFilesInternal(parent, fileAttributes).ToList(), token);
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
                    .Select(file => new FileInfo(file))
                    .Where(p => fileAttributes.Any(q => p.Attributes.HasFlag(q)))
                    .Select(info =>
                    {
                        var mimeType = string.Empty;
                        try
                        {
                            mimeType = GetMimeType(info);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to get mimetype from file");
                        }

                        return new ScarletFileInfo(info, mimeType ?? string.Empty);
                    });

                return fileInfos
                    .Select(p => new ScarletFile(_scheduler, p, parent, this));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Failed to get file info");

                _noAccessLookup.TryAdd(parent.FullName, parent.FullName);
            }

            return Enumerable.Empty<IFileSystemFile>();
        }
    }
}
