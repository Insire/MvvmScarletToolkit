using DynamicData;
using MvvmScarletToolkit.Wpf.Features.FileSystemBrowser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Wpf.FileSystemBrowser
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
                if (isEmpty)
                {
                    _viewModel._items.Clear();
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

            private void Set(ScarletDirectoryInfo info)
            {
                _viewModel.Name = info.Name;
                _viewModel.FullName = info.FullName;
                _viewModel.Exists = info.Exists;
                _viewModel.IsHidden = info.IsHidden;
                _viewModel.CreationTimeUtc = info.CreationTimeUtc;
                _viewModel.LastAccessTimeUtc = info.LastAccessTimeUtc;
                _viewModel.LastWriteTimeUtc = info.LastWriteTimeUtc;

                var index = 1;

                PropertyViewModel.AddUpdateOrUpdateCache(_viewModel._propertiesCache, index++, nameof(FileSystemType), _viewModel.FileSystemType.ToString());
                PropertyViewModel.AddUpdateOrUpdateCache(_viewModel._propertiesCache, index++, nameof(Name), info.Name);
                PropertyViewModel.AddUpdateOrUpdateCache(_viewModel._propertiesCache, index++, nameof(FullName), info.FullName);
                PropertyViewModel.AddUpdateOrUpdateCache(_viewModel._propertiesCache, index++, nameof(Exists), info.Exists ? bool.TrueString : bool.FalseString);
                PropertyViewModel.AddUpdateOrUpdateCache(_viewModel._propertiesCache, index++, nameof(IsHidden), info.IsHidden ? bool.TrueString : bool.FalseString);
                PropertyViewModel.AddUpdateOrUpdateCache(_viewModel._propertiesCache, index++, nameof(IsAccessProhibited), info.IsAccessProhibited ? bool.TrueString : bool.FalseString);
                PropertyViewModel.AddUpdateOrUpdateCache(_viewModel._propertiesCache, index++, nameof(CreationTimeUtc), info.CreationTimeUtc?.ToString() ?? string.Empty);
                PropertyViewModel.AddUpdateOrUpdateCache(_viewModel._propertiesCache, index++, nameof(LastAccessTimeUtc), info.LastAccessTimeUtc?.ToString() ?? string.Empty);
                PropertyViewModel.AddUpdateOrUpdateCache(_viewModel._propertiesCache, index++, nameof(LastWriteTimeUtc), info.LastWriteTimeUtc?.ToString() ?? string.Empty);
            }
        }
    }
}
