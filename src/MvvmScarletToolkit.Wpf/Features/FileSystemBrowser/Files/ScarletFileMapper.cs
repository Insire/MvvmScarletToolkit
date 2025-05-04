using MvvmScarletToolkit.Wpf.Features.FileSystemBrowser;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Wpf.FileSystemBrowser
{
    public sealed partial class ScarletFile
    {
        private sealed class ScarletFileMapper : IViewModelMapper
        {
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

            private void Set(ScarletFileInfo info)
            {
                _viewModel.Name = info.Name;
                _viewModel.FullName = info.FullName;
                _viewModel.Extension = info.Extension;
                _viewModel.MimeType = info.MimeType;
                _viewModel.Exists = info.Exists;
                _viewModel.IsHidden = info.IsHidden;
                _viewModel.CreationTimeUtc = info.CreationTimeUtc;
                _viewModel.LastAccessTimeUtc = info.LastAccessTimeUtc;
                _viewModel.LastWriteTimeUtc = info.LastWriteTimeUtc;

                var index = 1;

                PropertyViewModel.AddUpdateOrUpdateCache(_viewModel._propertiesCache, index++, nameof(FileSystemType), _viewModel.FileSystemType.ToString());
                PropertyViewModel.AddUpdateOrUpdateCache(_viewModel._propertiesCache, index++, nameof(MimeType), _viewModel.MimeType);
                PropertyViewModel.AddUpdateOrUpdateCache(_viewModel._propertiesCache, index++, nameof(Name), info.Name);
                PropertyViewModel.AddUpdateOrUpdateCache(_viewModel._propertiesCache, index++, nameof(FullName), info.FullName);
                PropertyViewModel.AddUpdateOrUpdateCache(_viewModel._propertiesCache, index++, nameof(Extension), info.Extension);
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
