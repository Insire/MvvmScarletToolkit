using CommunityToolkit.Mvvm.ComponentModel;
using MvvmScarletToolkit.Observables;
using MvvmScarletToolkit.Wpf.Features.FileSystemBrowser;
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
    public sealed partial class ScarletDrive : BusinessViewModelListBase<IFileSystemChild>, IFileSystemDrive
    {
        private readonly Mapper _mapper;
        private readonly IReadOnlyCollection<FileAttributes> _fileAttributes;
        private readonly IReadOnlyCollection<FileAttributes> _folderAttributes;

        [ObservableProperty, Bindable(true, BindingDirection.OneWay)] public partial string? DriveFormat { get; private set; }
        [ObservableProperty, Bindable(true, BindingDirection.OneWay)] public partial DriveType DriveType { get; private set; }
        [ObservableProperty, Bindable(true, BindingDirection.OneWay)] public partial bool IsReady { get; private set; }
        [ObservableProperty, Bindable(true, BindingDirection.OneWay)] public partial long AvailableFreeSpace { get; private set; }
        [ObservableProperty, Bindable(true, BindingDirection.OneWay)] public partial long TotalFreeSpace { get; private set; }
        [ObservableProperty, Bindable(true, BindingDirection.OneWay)] public partial long TotalSize { get; private set; }
        [ObservableProperty, Bindable(true, BindingDirection.OneWay)] public partial string Name { get; private set; }
        [ObservableProperty, Bindable(true, BindingDirection.OneWay)] public partial string FullName { get; private set; }
        [ObservableProperty, Bindable(true, BindingDirection.TwoWay)] public partial bool IsSelected { get; set; }

        [Bindable(true, BindingDirection.OneWay)] public bool IsContainer => true;

        public ScarletDrive(ScarletDriveInfo info, IScarletCommandBuilder commandBuilder, IFileSystemViewModelFactory factory, IReadOnlyCollection<FileAttributes> fileAttributes, IReadOnlyCollection<FileAttributes> folderAttributes)
            : base(commandBuilder)
        {
            ArgumentNullException.ThrowIfNull(info);


            _fileAttributes = fileAttributes;
            _folderAttributes = folderAttributes;
            _mapper = new Mapper(this, info, factory);
        }

        protected override Task RefreshInternal(CancellationToken token)
        {
            return _mapper.Refresh(token);
        }

        public sealed class Mapper : IViewModelMapper
        {
            private readonly ScarletDrive _viewModel;
            private readonly string _fullName;
            private readonly IFileSystemViewModelFactory _fileSystemViewModelFactory;

            public Mapper(ScarletDrive viewModel, ScarletDriveInfo info, IFileSystemViewModelFactory fileSystemViewModelFactory)
            {
                ArgumentNullException.ThrowIfNull(viewModel);
                ArgumentNullException.ThrowIfNull(info);
                ArgumentNullException.ThrowIfNull(fileSystemViewModelFactory);

                _viewModel = viewModel;
                _fileSystemViewModelFactory = fileSystemViewModelFactory;
                _fullName = info.FullName;
                Set(info);
            }

            private void Set(ScarletDriveInfo info)
            {
                _viewModel.Name = info.Name;
                _viewModel.FullName = info.FullName;
                _viewModel.DriveFormat = info.DriveFormat;
                _viewModel.DriveType = info.DriveType;
                _viewModel.IsReady = info.IsReady;
                _viewModel.AvailableFreeSpace = info.AvailableFreeSpace;
                _viewModel.TotalFreeSpace = info.TotalFreeSpace;
                _viewModel.TotalSize = info.TotalSize;
                _viewModel.FullName = info.FullName;
            }

            public async Task Refresh(CancellationToken token)
            {
                var info = await _fileSystemViewModelFactory.GetDriveInfo(_fullName, token);
                if (info is null)
                {
                    return;
                }

                Set(info);

                var isEmpty = await _fileSystemViewModelFactory.IsEmpty(_viewModel);
                if (isEmpty)
                {
                    await _viewModel.Clear(token);
                    return;
                }

                var children = await _fileSystemViewModelFactory.GetChildren(_viewModel, _viewModel._fileAttributes, _viewModel._folderAttributes);

                if (!_viewModel.IsLoaded)
                {
                    await _viewModel.AddRange(children, token);
                }
                else
                {
                    await _viewModel.Dispatcher.Invoke(() => _viewModel.Items.UpdateItems(children, FileSystemViewModelFactoryExtensions.IFileSystemChildComparer, FileSystemViewModelFactoryExtensions.IFileSystemChildMapper));
                }
            }
        }
    }
}
