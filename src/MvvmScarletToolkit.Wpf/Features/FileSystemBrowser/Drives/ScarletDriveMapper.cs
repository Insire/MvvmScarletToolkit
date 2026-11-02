using DynamicData;
using MvvmScarletToolkit.Wpf.Features.FileSystemBrowser.Interfaces;
using System.IO;

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

                var info = await Task.Run(() => _fileSystemViewModelFactory.GetDriveInfo(_fullName, cancellationToken), cancellationToken);
                if (info is null)
                {
                    _viewModel.IsAccessProhibited = true;
                    return;
                }

                Set(info);

                var isEmpty = await Task.Run(() => _fileSystemViewModelFactory.IsEmpty(_viewModel, cancellationToken), cancellationToken);
                if (isEmpty.HasValue)
                {
                    _viewModel.IsEmpty = isEmpty.Value;
                }
                else
                {
                    _viewModel.IsAccessProhibited = true;
                }

                if (_viewModel.IsEmpty)
                {
                    _viewModel._cache.Clear();
                    return;
                }

                var children = await Task.Run(() => _fileSystemViewModelFactory.GetChildren(_viewModel, _fileAttributes, _folderAttributes, cancellationToken), cancellationToken);

                if (!_viewModel.IsLoaded)
                {
                    _viewModel.IsLoaded = true;
                }

                _viewModel._cache.AddOrUpdate(children);
                _viewModel.HasChildContainers = children.Any(p => p.IsContainer);
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
