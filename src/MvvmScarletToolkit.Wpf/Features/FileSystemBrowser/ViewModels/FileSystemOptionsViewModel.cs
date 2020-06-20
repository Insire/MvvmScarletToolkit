using MvvmScarletToolkit.Observables;
using System;
using System.Collections.Generic;
using System.IO;

namespace MvvmScarletToolkit.Wpf.FileSystemBrowser
{
    public sealed class FileSystemOptionsViewModel : ObservableObject
    {
        private static readonly Lazy<FileSystemOptionsViewModel> _default = new Lazy<FileSystemOptionsViewModel>(() => new FileSystemOptionsViewModel(new[] { DriveType.Fixed }, FileAttributes.Normal, FileAttributes.Directory));

        public static FileSystemOptionsViewModel Default => _default.Value;

        public IReadOnlyCollection<DriveType> DriveTypes { get; }
        public FileAttributes FileAttributes { get; }
        public FileAttributes FolderAttributes { get; }

        public FileSystemOptionsViewModel(IReadOnlyCollection<DriveType> driveTypes, FileAttributes fileAttributes, FileAttributes folderAttributes)
        {
            DriveTypes = driveTypes ?? throw new ArgumentNullException(nameof(driveTypes));
            FileAttributes = fileAttributes;
            FolderAttributes = folderAttributes;
        }
    }
}
