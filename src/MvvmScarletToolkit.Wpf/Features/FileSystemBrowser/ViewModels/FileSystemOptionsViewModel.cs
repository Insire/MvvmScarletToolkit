using MvvmScarletToolkit.Observables;
using System;
using System.Collections.Generic;
using System.IO;

namespace MvvmScarletToolkit.Wpf.FileSystemBrowser
{
    public sealed class FileSystemOptionsViewModel : ObservableObject
    {
        private static readonly Lazy<FileSystemOptionsViewModel> _default = new Lazy<FileSystemOptionsViewModel>(() => new FileSystemOptionsViewModel(new[] { DriveType.Fixed },
            new[]
            {
                System.IO.FileAttributes.Archive,
                System.IO.FileAttributes.Archive | System.IO.FileAttributes.Normal,
                System.IO.FileAttributes.Archive | System.IO.FileAttributes.Compressed
            },
            new[]
            {
                System.IO.FileAttributes.Directory,
            }));

        public static FileSystemOptionsViewModel Default => _default.Value;

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
