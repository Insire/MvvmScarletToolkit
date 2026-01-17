using DynamicData;
using MvvmScarletToolkit.Wpf.Features.FileSystemBrowser.Interfaces;
using System.IO;

namespace MvvmScarletToolkit.Wpf.Features.FileSystemBrowser.Directories
{
    public sealed partial class ScarletDirectory
    {
        private sealed class ScarletDirectoryMapper : IViewModelMapper
        {
            private readonly ScarletDirectory _viewModel;
            private readonly IReadOnlyCollection<FileAttributes> _fileAttributes;
            private readonly IReadOnlyCollection<FileAttributes> _folderAttributes;
            private readonly string _fullName;
            private readonly IFileSystemViewModelFactory _fileSystemViewModelFactory;

            public ScarletDirectoryMapper(
                ScarletDirectory viewModel,
                ScarletDirectoryInfo info,
                IReadOnlyCollection<FileAttributes> fileAttributes,
                IReadOnlyCollection<FileAttributes> folderAttributes,
                IFileSystemViewModelFactory fileSystemViewModelFactory)
            {
                ArgumentNullException.ThrowIfNull(viewModel);
                ArgumentNullException.ThrowIfNull(info);
                ArgumentNullException.ThrowIfNull(fileSystemViewModelFactory);

                _viewModel = viewModel;
                _fileAttributes = fileAttributes;
                _folderAttributes = folderAttributes;
                _fileSystemViewModelFactory = fileSystemViewModelFactory;
                _fullName = info.FullName;

                Set(info);
            }

            public async Task Refresh(CancellationToken cancellationToken)
            {
                using var token = _viewModel._busyStack.GetToken();

                var info = await _fileSystemViewModelFactory.GetDirectoryInfo(_fullName, cancellationToken);
                if (info is null)
                {
                    _viewModel.IsAccessProhibited = true;
                    return;
                }

                Set(info);

                var isEmpty = await _fileSystemViewModelFactory.IsEmpty(_viewModel, cancellationToken);
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
                    _viewModel._items.Clear();
                    return;
                }

                var children = await _fileSystemViewModelFactory.GetChildren(_viewModel, _fileAttributes, _folderAttributes, cancellationToken);

                if (!_viewModel.IsLoaded)
                {
                    _viewModel.IsLoaded = true;
                }

                _viewModel._cache.AddOrUpdate(children);
                _viewModel.HasChildContainers = children.Any(p => p.IsContainer);
            }

            private void Set(ScarletDirectoryInfo info)
            {
                _viewModel.Name = info.Name;
                _viewModel.FullName = info.FullName;
                _viewModel.Exists = info.Exists;
                _viewModel.IsHidden = info.IsHidden;
                _viewModel.CreationTimeUtc = info.CreationTimeUtc;
                _viewModel.LastAccessTimeUtc = info.LastAccessTimeUtc;
                _viewModel.LastWriteTimeUtc = info.LastWriteTimeUtc;

                var index = 0;

                PropertyViewModel.AddUpdateOrUpdateCache(_viewModel._propertiesCache, ++index, nameof(FileSystemType), _viewModel.FileSystemType.ToString());
                PropertyViewModel.AddUpdateOrUpdateCache(_viewModel._propertiesCache, ++index, nameof(Name), info.Name);
                PropertyViewModel.AddUpdateOrUpdateCache(_viewModel._propertiesCache, ++index, nameof(FullName), info.FullName);
                PropertyViewModel.AddUpdateOrUpdateCache(_viewModel._propertiesCache, ++index, nameof(Exists), info.Exists ? bool.TrueString : bool.FalseString);
                PropertyViewModel.AddUpdateOrUpdateCache(_viewModel._propertiesCache, ++index, nameof(IsHidden), info.IsHidden ? bool.TrueString : bool.FalseString);
                PropertyViewModel.AddUpdateOrUpdateCache(_viewModel._propertiesCache, ++index, nameof(IsAccessProhibited), info.IsAccessProhibited ? bool.TrueString : bool.FalseString);
                PropertyViewModel.AddUpdateOrUpdateCache(_viewModel._propertiesCache, ++index, nameof(CreationTimeUtc), info.CreationTimeUtc?.ToString() ?? string.Empty);
                PropertyViewModel.AddUpdateOrUpdateCache(_viewModel._propertiesCache, ++index, nameof(LastAccessTimeUtc), info.LastAccessTimeUtc?.ToString() ?? string.Empty);
                PropertyViewModel.AddUpdateOrUpdateCache(_viewModel._propertiesCache, ++index, nameof(LastWriteTimeUtc), info.LastWriteTimeUtc?.ToString() ?? string.Empty);
            }
        }
    }
}
