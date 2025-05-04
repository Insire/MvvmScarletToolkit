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

            public async Task Refresh(CancellationToken token)
            {
                var info = await _fileSystemViewModelFactory.GetDriveInfo(_fullName, token);
                if (info is null)
                {
                    _viewModel.IsAccessProhibited = true;
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

                var found = _viewModel._propertiesCache.Lookup(nameof(Name));
                if (found.HasValue)
                {
                    found.Value.Value = _viewModel.Name;
                }
                else
                {
                    _viewModel._propertiesCache.AddOrUpdate(PropertyViewModel.Create(nameof(Name), info.Name, 1));
                }

                found = _viewModel._propertiesCache.Lookup(nameof(FullName));
                if (found.HasValue)
                {
                    found.Value.Value = _viewModel.FullName;
                }
                else
                {
                    _viewModel._propertiesCache.AddOrUpdate(PropertyViewModel.Create(nameof(FullName), info.FullName, 2));
                }

                found = _viewModel._propertiesCache.Lookup(nameof(IsAccessProhibited));
                if (found.HasValue)
                {
                    found.Value.Value = _viewModel.IsAccessProhibited ? bool.TrueString : bool.FalseString;
                }
                else
                {
                    _viewModel._propertiesCache.AddOrUpdate(PropertyViewModel.Create(nameof(IsAccessProhibited), info.IsAccessProhibited ? bool.TrueString : bool.FalseString, 3));
                }

                found = _viewModel._propertiesCache.Lookup(nameof(DriveFormat));
                if (found.HasValue)
                {
                    found.Value.Value = _viewModel.DriveFormat ?? string.Empty;
                }
                else
                {
                    _viewModel._propertiesCache.AddOrUpdate(PropertyViewModel.Create(nameof(DriveFormat), info.DriveFormat ?? string.Empty, 4));
                }

                found = _viewModel._propertiesCache.Lookup(nameof(DriveType));
                if (found.HasValue)
                {
                    found.Value.Value = _viewModel.DriveType.ToString();
                }
                else
                {
                    _viewModel._propertiesCache.AddOrUpdate(PropertyViewModel.Create(nameof(DriveType), info.DriveType.ToString(), 5));
                }

                found = _viewModel._propertiesCache.Lookup(nameof(AvailableFreeSpace));
                if (found.HasValue)
                {
                    found.Value.Value = _viewModel.AvailableFreeSpace.ToString();
                }
                else
                {
                    _viewModel._propertiesCache.AddOrUpdate(PropertyViewModel.Create(nameof(AvailableFreeSpace), info.AvailableFreeSpace.ToString(), 6));
                }

                found = _viewModel._propertiesCache.Lookup(nameof(TotalFreeSpace));
                if (found.HasValue)
                {
                    found.Value.Value = _viewModel.TotalFreeSpace.ToString();
                }
                else
                {
                    _viewModel._propertiesCache.AddOrUpdate(PropertyViewModel.Create(nameof(TotalFreeSpace), info.TotalFreeSpace.ToString(), 7));
                }

                found = _viewModel._propertiesCache.Lookup(nameof(TotalSize));
                if (found.HasValue)
                {
                    found.Value.Value = _viewModel.TotalSize.ToString();
                }
                else
                {
                    _viewModel._propertiesCache.AddOrUpdate(PropertyViewModel.Create(nameof(TotalSize), info.TotalSize.ToString(), 8));
                }

                found = _viewModel._propertiesCache.Lookup(nameof(IsReady));
                if (found.HasValue)
                {
                    found.Value.Value = _viewModel.IsReady ? bool.TrueString : bool.FalseString;
                }
                else
                {
                    _viewModel._propertiesCache.AddOrUpdate(PropertyViewModel.Create(nameof(IsReady), info.IsReady ? bool.TrueString : bool.FalseString, 3));
                }
            }
        }
    }
}
