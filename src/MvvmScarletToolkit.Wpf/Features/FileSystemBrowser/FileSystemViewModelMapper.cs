using CommunityToolkit.Mvvm.Messaging;
using DynamicData;
using MvvmScarletToolkit.Wpf.Features.FileSystemBrowser.Directories;
using MvvmScarletToolkit.Wpf.Features.FileSystemBrowser.Drives;
using MvvmScarletToolkit.Wpf.Features.FileSystemBrowser.Files;
using MvvmScarletToolkit.Wpf.Features.FileSystemBrowser.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Wpf.Features.FileSystemBrowser
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
                _viewModel.SelectedTreeDetail = message.Directory;
                _viewModel.SelectedDetail = message.Directory;
                _viewModel.SelectedDirectory = message.Directory;

                _viewModel.SelectedFile = null;
            }

            public void Receive(ScarletFileSelected message)
            {
                _viewModel.SelectedChild = message.File;
                _viewModel.SelectedDetail = message.File;
                _viewModel.SelectedFile = message.File;

                _viewModel.SelectedDirectory = null;
            }

            public void Receive(ScarletDriveSelected message)
            {
                _viewModel.SelectedContainer = message.Drive;
                _viewModel.SelectedTreeDetail = message.Drive;

                _viewModel.SelectedChild = null;
                _viewModel.SelectedFile = null;
                _viewModel.SelectedDirectory = null;
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
