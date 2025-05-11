using DynamicData;
using MvvmScarletToolkit.Wpf.Features.FileSystemBrowser.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Wpf.Features.FileSystemBrowser.Drives
{
    public sealed partial class ScarletDrive
    {
        private sealed class ScarletDriveMapper : IViewModelMapper
        {
            private readonly ScarletDrive _viewModel;
            private readonly string _fullName;
            private readonly IReadOnlyCollection<FileAttributes> _fileAttributes;
            private readonly IReadOnlyCollection<FileAttributes> _folderAttributes;
            private readonly IFileSystemViewModelFactory _fileSystemViewModelFactory;

            public ScarletDriveMapper(
                ScarletDrive viewModel,
                ScarletDriveInfo info,
                IFileSystemViewModelFactory fileSystemViewModelFactory,
                IReadOnlyCollection<FileAttributes> fileAttributes,
                IReadOnlyCollection<FileAttributes> folderAttributes)
            {
                ArgumentNullException.ThrowIfNull(viewModel);
                ArgumentNullException.ThrowIfNull(info);
                ArgumentNullException.ThrowIfNull(fileSystemViewModelFactory);

                _viewModel = viewModel;
                _fileSystemViewModelFactory = fileSystemViewModelFactory;
                _fileAttributes = fileAttributes;
                _folderAttributes = folderAttributes;
                _fullName = info.FullName;

                Set(info);
            }

            public async Task Refresh(CancellationToken cancellationToken)
            {
                using var token = _viewModel._busyStack.GetToken();

                var info = await _fileSystemViewModelFactory.GetDriveInfo(_fullName, cancellationToken);
                if (info is null)
                {
                    _viewModel.IsAccessProhibited = true;
                    return;
                }

                Set(info);

                var isEmpty = await _fileSystemViewModelFactory.IsEmpty(_viewModel, cancellationToken);
                if (isEmpty)
                {
                    _viewModel._cache.Clear();
                    return;
                }

                var children = await _fileSystemViewModelFactory.GetChildren(_viewModel, _fileAttributes, _folderAttributes, cancellationToken);

                if (!_viewModel.IsLoaded)
                {
                    _viewModel._cache.AddOrUpdate(children);
                    _viewModel.IsLoaded = true;
                }
                else
                {
                    _viewModel._cache.AddOrUpdate(children);
                }
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

                var index = 1;

                PropertyViewModel.AddUpdateOrUpdateCache(_viewModel._propertiesCache, index++, nameof(FileSystemType), _viewModel.FileSystemType.ToString());
                PropertyViewModel.AddUpdateOrUpdateCache(_viewModel._propertiesCache, index++, nameof(Name), info.Name);
                PropertyViewModel.AddUpdateOrUpdateCache(_viewModel._propertiesCache, index++, nameof(FullName), info.FullName);
                PropertyViewModel.AddUpdateOrUpdateCache(_viewModel._propertiesCache, index++, nameof(DriveFormat), info.DriveFormat ?? string.Empty);
                PropertyViewModel.AddUpdateOrUpdateCache(_viewModel._propertiesCache, index++, nameof(DriveType), info.DriveType.ToString());
                PropertyViewModel.AddUpdateOrUpdateCache(_viewModel._propertiesCache, index++, nameof(Exists), _viewModel.Exists ? bool.TrueString : bool.FalseString);
                PropertyViewModel.AddUpdateOrUpdateCache(_viewModel._propertiesCache, index++, nameof(IsReady), info.IsReady ? bool.TrueString : bool.FalseString);
                PropertyViewModel.AddUpdateOrUpdateCache(_viewModel._propertiesCache, index++, nameof(IsBusy), _viewModel.IsBusy ? bool.TrueString : bool.FalseString);

                PropertyViewModel.AddUpdateOrUpdateCache(_viewModel._propertiesCache, index++, nameof(IsAccessProhibited), info.IsAccessProhibited ? bool.TrueString : bool.FalseString);
                PropertyViewModel.AddUpdateOrUpdateCache(_viewModel._propertiesCache, index++, nameof(AvailableFreeSpace), info.AvailableFreeSpace.ToString());
                PropertyViewModel.AddUpdateOrUpdateCache(_viewModel._propertiesCache, index++, nameof(TotalFreeSpace), info.TotalFreeSpace.ToString());
                PropertyViewModel.AddUpdateOrUpdateCache(_viewModel._propertiesCache, index++, nameof(TotalSize), info.TotalSize.ToString());
            }
        }
    }
}
