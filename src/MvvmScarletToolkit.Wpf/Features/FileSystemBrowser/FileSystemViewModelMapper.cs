using CommunityToolkit.Mvvm.Messaging;
using DynamicData;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Wpf.FileSystemBrowser
{
    public sealed partial class FileSystemViewModel
    {
        public sealed class FileSystemViewModelMapper : IViewModelMapper, IRecipient<ScarletDirectorySelected>, IRecipient<ScarletFileSelected>, IRecipient<ScarletDriveSelected>
        {
            private readonly FileSystemViewModel _viewModel;
            private readonly IFileSystemViewModelFactory _fileSystemViewModelFactory;

            public FileSystemViewModelMapper(
                FileSystemViewModel viewModel,
                IFileSystemViewModelFactory fileSystemViewModelFactory)
            {
                ArgumentNullException.ThrowIfNull(viewModel);
                ArgumentNullException.ThrowIfNull(fileSystemViewModelFactory);

                _viewModel = viewModel;
                _fileSystemViewModelFactory = fileSystemViewModelFactory;
            }

            public void Receive(ScarletDirectorySelected message)
            {
                _viewModel.SelectedChild = message.Directory;
                _viewModel.SelectedContainer = message.Directory;
            }

            public void Receive(ScarletFileSelected message)
            {
                _viewModel.SelectedChild = message.File;
            }

            public void Receive(ScarletDriveSelected message)
            {
                _viewModel.SelectedContainer = message.Drive;
            }

            public async Task Refresh(CancellationToken token)
            {
                var children = await _fileSystemViewModelFactory.GetDrives(_viewModel.Options.DriveTypes, _viewModel.Options.FileAttributes, _viewModel.Options.FolderAttributes, token);

                if (!_viewModel.IsLoaded)
                {
                    _viewModel._cache.AddOrUpdate(children);
                    _viewModel.IsLoaded = true;

                    _fileSystemViewModelFactory.Messenger.RegisterAll(this);
                }
                else
                {
                    _viewModel._cache.AddOrUpdate(children);
                }
            }
        }
    }
}
