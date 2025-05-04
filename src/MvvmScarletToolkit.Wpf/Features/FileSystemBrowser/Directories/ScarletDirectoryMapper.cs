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

            public async Task Refresh(CancellationToken token)
            {
                var info = await _fileSystemViewModelFactory.GetDirectoryInfo(_fullName, token);
                if (info is null)
                {
                    _viewModel.IsAccessProhibited = true;
                    return;
                }

                Set(info);

                var isEmpty = await _fileSystemViewModelFactory.IsEmpty(_viewModel, token);
                if (isEmpty)
                {
                    _viewModel._items.Clear();
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

            private void Set(ScarletDirectoryInfo info)
            {
                _viewModel.Name = info.Name;
                _viewModel.FullName = info.FullName;
                _viewModel.Exists = info.Exists;
                _viewModel.IsHidden = info.IsHidden;
                _viewModel.CreationTimeUtc = info.CreationTimeUtc;
                _viewModel.LastAccessTimeUtc = info.LastAccessTimeUtc;
                _viewModel.LastWriteTimeUtc = info.LastWriteTimeUtc;

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

                found = _viewModel._propertiesCache.Lookup(nameof(Exists));
                if (found.HasValue)
                {
                    found.Value.Value = _viewModel.Exists ? bool.TrueString : bool.FalseString;
                }
                else
                {
                    _viewModel._propertiesCache.AddOrUpdate(PropertyViewModel.Create(nameof(Exists), info.Exists ? bool.TrueString : bool.FalseString, 3));
                }

                found = _viewModel._propertiesCache.Lookup(nameof(IsHidden));
                if (found.HasValue)
                {
                    found.Value.Value = _viewModel.IsHidden ? bool.TrueString : bool.FalseString;
                }
                else
                {
                    _viewModel._propertiesCache.AddOrUpdate(PropertyViewModel.Create(nameof(IsHidden), info.IsHidden ? bool.TrueString : bool.FalseString, 4));
                }

                found = _viewModel._propertiesCache.Lookup(nameof(IsAccessProhibited));
                if (found.HasValue)
                {
                    found.Value.Value = _viewModel.IsAccessProhibited ? bool.TrueString : bool.FalseString;
                }
                else
                {
                    _viewModel._propertiesCache.AddOrUpdate(PropertyViewModel.Create(nameof(IsAccessProhibited), info.IsAccessProhibited ? bool.TrueString : bool.FalseString, 4));
                }

                found = _viewModel._propertiesCache.Lookup(nameof(CreationTimeUtc));
                if (found.HasValue)
                {
                    found.Value.Value = _viewModel.CreationTimeUtc?.ToString() ?? string.Empty;
                }
                else
                {
                    _viewModel._propertiesCache.AddOrUpdate(PropertyViewModel.Create(nameof(CreationTimeUtc), info.CreationTimeUtc?.ToString() ?? string.Empty, 5));
                }

                found = _viewModel._propertiesCache.Lookup(nameof(LastAccessTimeUtc));
                if (found.HasValue)
                {
                    found.Value.Value = _viewModel.LastAccessTimeUtc?.ToString() ?? string.Empty;
                }
                else
                {
                    _viewModel._propertiesCache.AddOrUpdate(PropertyViewModel.Create(nameof(LastAccessTimeUtc), info.LastAccessTimeUtc?.ToString() ?? string.Empty, 6));
                }

                found = _viewModel._propertiesCache.Lookup(nameof(LastWriteTimeUtc));
                if (found.HasValue)
                {
                    found.Value.Value = _viewModel.LastWriteTimeUtc?.ToString() ?? string.Empty;
                }
                else
                {
                    _viewModel._propertiesCache.AddOrUpdate(PropertyViewModel.Create(nameof(LastWriteTimeUtc), info.LastWriteTimeUtc?.ToString() ?? string.Empty, 7));
                }
            }
        }
    }
}
