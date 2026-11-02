using CommunityToolkit.Mvvm.Messaging;
using DynamicData;
using MvvmScarletToolkit.Wpf.Features.FileSystemBrowser.Directories;
using MvvmScarletToolkit.Wpf.Features.FileSystemBrowser.Drives;
using MvvmScarletToolkit.Wpf.Features.FileSystemBrowser.Files;
using MvvmScarletToolkit.Wpf.Features.FileSystemBrowser.Interfaces;

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
                if (!IsOwnEntry(message.Directory))
                {
                    return;
                }

                _viewModel.SelectedChild = message.Directory;
                _viewModel.SelectedContainer = message.Directory;
                _viewModel.SelectedTreeDetail = message.Directory;
                _viewModel.SelectedDetail = message.Directory;
                _viewModel.SelectedDirectory = message.Directory;

                _viewModel.SelectedFile = null;
                _viewModel.OnContainerSelected(message.Directory);
            }

            public void Receive(ScarletFileSelected message)
            {
                if (!IsOwnEntry(message.File))
                {
                    return;
                }

                _viewModel.SelectedChild = message.File;
                _viewModel.SelectedDetail = message.File;
                _viewModel.SelectedFile = message.File;

                _viewModel.SelectedDirectory = null;
            }

            public void Receive(ScarletDriveSelected message)
            {
                if (!IsOwnEntry(message.Drive))
                {
                    return;
                }

                _viewModel.SelectedContainer = message.Drive;
                _viewModel.SelectedTreeDetail = message.Drive;

                _viewModel.SelectedChild = null;
                _viewModel.SelectedFile = null;
                _viewModel.SelectedDirectory = null;
                _viewModel.OnContainerSelected(message.Drive);
            }

            /// <summary>
            /// All viewmodels of a factory share its messenger, so an entry has to lead to one of the own drives.
            /// </summary>
            private bool IsOwnEntry(IFileSystemInfo entry)
            {
                while (entry is IFileSystemChild { Parent: not null } child)
                {
                    entry = child.Parent;
                }

                if (entry is not IFileSystemDrive drive)
                {
                    return false;
                }

                var ownDrive = _viewModel._cache.Lookup(drive.Name);

                return ownDrive.HasValue && ReferenceEquals(ownDrive.Value, drive);
            }

            public async Task Refresh(CancellationToken token)
            {
                var children = await _fileSystemViewModelFactory.GetDrives(_viewModel.Options.DriveTypes, _viewModel.Options.FileAttributes, _viewModel.Options.FolderAttributes, token);

                if (!_viewModel.IsLoaded)
                {
                    _viewModel._cache.AddOrUpdate(children);
                    _viewModel.IsLoaded = true;

                    _fileSystemViewModelFactory.Messenger.RegisterAll(this);

                    if (_viewModel.InitialFolder is not null)
                    {
                        await _viewModel.OpenFolder(_viewModel.InitialFolder);
                    }
                }
                else
                {
                    _viewModel._cache.AddOrUpdate(children);
                }
            }
        }
    }
}
