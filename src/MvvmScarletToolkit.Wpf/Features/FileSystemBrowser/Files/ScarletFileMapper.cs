using MvvmScarletToolkit.Wpf.Features.FileSystemBrowser.Interfaces;
using System.IO;

namespace MvvmScarletToolkit.Wpf.Features.FileSystemBrowser.Files
{
    public sealed partial class ScarletFile
    {
        private sealed class ScarletFileMapper : IViewModelMapper
        {
            private const int MimeTypePropertyIndex = 2;
            private const int ImagePropertyIndex = 8;

            private readonly ScarletFile _viewModel;
            private readonly string _fullName;
            private readonly IFileSystemViewModelFactory _fileSystemViewModelFactory;

            public ScarletFileMapper(ScarletFile viewModel, ScarletFileInfo info, IFileSystemViewModelFactory fileSystemViewModelFactory)
            {
                _viewModel = viewModel;
                _fileSystemViewModelFactory = fileSystemViewModelFactory;
                _fullName = info.FullName;

                Set(info);
            }

            public async Task Refresh(CancellationToken cancellationToken)
            {
                using var token = _viewModel._busyStack.GetToken();

                var info = await _fileSystemViewModelFactory.GetFileInfo(_fullName, cancellationToken);
                if (info is null)
                {
                    _viewModel.IsAccessProhibited = true;
                    return;
                }

                Set(info);
            }

            public async Task CheckHeader(CancellationToken cancellationToken)
            {
                if (_viewModel.IsHeaderChecked)
                {
                    return;
                }

                var header = await _fileSystemViewModelFactory.GetFileHeader(_fullName, cancellationToken);
                if (!header.IsReadable)
                {
                    _viewModel.IsAccessProhibited = true;
                }

                if (!string.IsNullOrEmpty(header.MimeType))
                {
                    SetMimeType(header.MimeType);
                }

                _viewModel.IsImage = header.MimeType?.StartsWith("image/", StringComparison.OrdinalIgnoreCase) == true;
                _viewModel.IsHeaderChecked = true;
                PropertyViewModel.AddUpdateOrUpdateCache(_viewModel._propertiesCache, ImagePropertyIndex, nameof(IsImage), _viewModel.IsImage ? bool.TrueString : bool.FalseString);
            }

            private void SetMimeType(string mimeType)
            {
                _viewModel.MimeType = mimeType;
                PropertyViewModel.AddUpdateOrUpdateCache(_viewModel._propertiesCache, MimeTypePropertyIndex, nameof(MimeType), mimeType);
            }

            private void Set(ScarletFileInfo info)
            {
                _viewModel.Name = info.Name;
                _viewModel.FullName = info.FullName;
                _viewModel.Extension = info.Extension;
                _viewModel.Location = Path.GetDirectoryName(info.FullName) ?? string.Empty;
                _viewModel.Length = info.Length;
                _viewModel.Attributes = info.Attributes;
                _viewModel.Exists = info.Exists;
                _viewModel.IsHidden = info.IsHidden;
                _viewModel.CreationTimeUtc = info.CreationTimeUtc;
                _viewModel.LastAccessTimeUtc = info.LastAccessTimeUtc;
                _viewModel.LastWriteTimeUtc = info.LastWriteTimeUtc;

                // the header outranks the extension, a refresh must not undo a finished check
                if (!_viewModel.IsHeaderChecked)
                {
                    SetMimeType(info.MimeType);
                }

                var index = 1;

                PropertyViewModel.AddUpdateOrUpdateCache(_viewModel._propertiesCache, index, nameof(FileSystemType), _viewModel.FileSystemType.ToString());
                index = MimeTypePropertyIndex + 1;
                PropertyViewModel.AddUpdateOrUpdateCache(_viewModel._propertiesCache, index++, nameof(Name), info.Name);
                PropertyViewModel.AddUpdateOrUpdateCache(_viewModel._propertiesCache, index++, nameof(FullName), info.FullName);
                PropertyViewModel.AddUpdateOrUpdateCache(_viewModel._propertiesCache, index++, nameof(Extension), info.Extension);
                PropertyViewModel.AddUpdateOrUpdateCache(_viewModel._propertiesCache, index++, nameof(Exists), info.Exists ? bool.TrueString : bool.FalseString);
                PropertyViewModel.AddUpdateOrUpdateCache(_viewModel._propertiesCache, index++, nameof(IsHidden), info.IsHidden ? bool.TrueString : bool.FalseString);
                PropertyViewModel.AddUpdateOrUpdateCache(_viewModel._propertiesCache, index++, nameof(IsImage), _viewModel.IsImage ? bool.TrueString : bool.FalseString);
                PropertyViewModel.AddUpdateOrUpdateCache(_viewModel._propertiesCache, index++, nameof(IsAccessProhibited), info.IsAccessProhibited ? bool.TrueString : bool.FalseString);
                PropertyViewModel.AddUpdateOrUpdateCache(_viewModel._propertiesCache, index++, nameof(CreationTimeUtc), info.CreationTimeUtc?.ToString() ?? string.Empty);
                PropertyViewModel.AddUpdateOrUpdateCache(_viewModel._propertiesCache, index++, nameof(LastAccessTimeUtc), info.LastAccessTimeUtc?.ToString() ?? string.Empty);
                PropertyViewModel.AddUpdateOrUpdateCache(_viewModel._propertiesCache, index++, nameof(LastWriteTimeUtc), info.LastWriteTimeUtc?.ToString() ?? string.Empty);
            }
        }
    }
}
