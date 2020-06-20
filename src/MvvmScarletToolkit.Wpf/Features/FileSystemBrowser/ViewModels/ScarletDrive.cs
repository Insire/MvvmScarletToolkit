using MvvmScarletToolkit.Observables;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Wpf.FileSystemBrowser
{
    [DebuggerDisplay("Drive: {Name} IsContainer: {IsContainer}")]
    public sealed class ScarletDrive : BusinessViewModelListBase<IFileSystemChild>, IFileSystemDrive
    {
        private readonly IFileSystemViewModelFactory _factory;
        private readonly IReadOnlyCollection<FileAttributes> _fileAttributes;
        private readonly IReadOnlyCollection<FileAttributes> _folderAttributes;

        private string? _driveFormat;
        [Bindable(true, BindingDirection.OneWay)]
        public string? DriveFormat
        {
            get { return _driveFormat; }
            private set { SetValue(ref _driveFormat, value); }
        }

        private DriveType _driveType;
        [Bindable(true, BindingDirection.OneWay)]
        public DriveType DriveType
        {
            get { return _driveType; }
            private set { SetValue(ref _driveType, value); }
        }

        private bool _isReady;
        [Bindable(true, BindingDirection.OneWay)]
        public bool IsReady
        {
            get { return _isReady; }
            private set { SetValue(ref _isReady, value); }
        }

        private long _availableFreeSpace;
        [Bindable(true, BindingDirection.OneWay)]
        public long AvailableFreeSpace
        {
            get { return _availableFreeSpace; }
            private set { SetValue(ref _availableFreeSpace, value); }
        }

        private long _totalFreeSpace;
        [Bindable(true, BindingDirection.OneWay)]
        public long TotalFreeSpace
        {
            get { return _totalFreeSpace; }
            private set { SetValue(ref _totalFreeSpace, value); }
        }

        private long _totalSize;
        [Bindable(true, BindingDirection.OneWay)]
        public long TotalSize
        {
            get { return _totalSize; }
            private set { SetValue(ref _totalSize, value); }
        }

        private string _name;
        [Bindable(true, BindingDirection.OneWay)]
        public string Name
        {
            get { return _name; }
            private set { SetValue(ref _name, value); }
        }

        private string _fullName;
        [Bindable(true, BindingDirection.OneWay)]
        public string FullName
        {
            get { return _fullName; }
            private set { SetValue(ref _fullName, value); }
        }

        private bool _isSelected;
        [Bindable(true, BindingDirection.TwoWay)]
        public bool IsSelected
        {
            get { return _isSelected; }
            set { SetValue(ref _isSelected, value); }
        }

        [Bindable(true, BindingDirection.OneWay)]
        public bool IsContainer { get; }

        public ScarletDrive(DriveInfo info, IScarletCommandBuilder commandBuilder, IFileSystemViewModelFactory factory, IReadOnlyCollection<FileAttributes> fileAttributes, IReadOnlyCollection<FileAttributes> folderAttributes)
            : base(commandBuilder)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _name = info.Name ?? throw new ArgumentException(nameof(info.Name));

            _fullName = info.Name;
            _fileAttributes = fileAttributes;
            _folderAttributes = folderAttributes;

            IsContainer = true;

            Set(info);
        }

        private void Set(DriveInfo info)
        {
            DriveFormat = info.DriveFormat;
            DriveType = info.DriveType;
            IsReady = info.IsReady;
            AvailableFreeSpace = info.AvailableFreeSpace;
            TotalFreeSpace = info.TotalFreeSpace;
            TotalSize = info.TotalSize;
        }

        protected override async Task RefreshInternal(CancellationToken token)
        {
            var info = await Task.Run(() => new DriveInfo(FullName), token);
            await Dispatcher.Invoke(() => Set(info));

            var isEmpty = await _factory.IsEmpty(this);
            if (isEmpty)
            {
                await Clear(token);
                return;
            }

            var children = await _factory.GetChildren(this, _fileAttributes, _folderAttributes);

            if (!IsLoaded)
            {
                await AddRange(children);
            }
            else
            {
                await Dispatcher.Invoke(() => Items.UpdateItems(children, FileSystemViewModelFactoryExtensions.IFileSystemChildComparer, FileSystemViewModelFactoryExtensions.IFileSystemChildMapper));
            }
        }
    }
}
