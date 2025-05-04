using DynamicData;
using MvvmScarletToolkit.Wpf.Features.FileSystemBrowser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Wpf.FileSystemBrowser
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

                var isEmpty = await _fileSystemViewModelFactory.IsEmpty(_viewModel, token);
                if (isEmpty)
                {
                    _viewModel._cache.Clear();
                    return;
                }

                var children = await _fileSystemViewModelFactory.GetChildren(_viewModel, _fileAttributes, _folderAttributes, token);

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
        }
    }
}
