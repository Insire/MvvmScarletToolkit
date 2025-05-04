using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.IO;

namespace MvvmScarletToolkit.Wpf.FileSystemBrowser
{
    public sealed class FileSystemOptionsViewModel : ObservableObject
    {
        private static readonly Lazy<FileSystemOptionsViewModel> _default;

        public static FileSystemOptionsViewModel Default => _default.Value;

        static FileSystemOptionsViewModel()
        {
            var driveTypes = new[]
            {
                DriveType.Fixed
            };

            var fileAttributes = new[]
            {
                System.IO.FileAttributes.Archive,
                System.IO.FileAttributes.Archive | System.IO.FileAttributes.Normal,
                System.IO.FileAttributes.Archive | System.IO.FileAttributes.Compressed
            };

            var folderAttributes = new[]
            {
                System.IO.FileAttributes.Directory,
            };

            _default = new Lazy<FileSystemOptionsViewModel>(() => new FileSystemOptionsViewModel(driveTypes, fileAttributes, folderAttributes));
        }

        public IReadOnlyCollection<DriveType> DriveTypes { get; }
        public IReadOnlyCollection<FileAttributes> FileAttributes { get; }
        public IReadOnlyCollection<FileAttributes> FolderAttributes { get; }

        public FileSystemOptionsViewModel(IReadOnlyCollection<DriveType> driveTypes, IReadOnlyCollection<FileAttributes> fileAttributes, IReadOnlyCollection<FileAttributes> folderAttributes)
        {
            DriveTypes = driveTypes ?? throw new ArgumentNullException(nameof(driveTypes));
            FileAttributes = fileAttributes ?? throw new ArgumentNullException(nameof(fileAttributes));
            FolderAttributes = folderAttributes ?? throw new ArgumentNullException(nameof(folderAttributes));
        }
    }
}
