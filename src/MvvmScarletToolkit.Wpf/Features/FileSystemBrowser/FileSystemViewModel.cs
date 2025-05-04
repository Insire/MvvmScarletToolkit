using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Wpf.FileSystemBrowser
{
    public sealed partial class FileSystemViewModel : ObservableObject
    {
        private readonly ObservableCollection<IFileSystemDrive> _items;
        private readonly Mapper _mapper;

        [ObservableProperty, Bindable(true, BindingDirection.OneWay)] public partial bool IsLoaded { get; private set; }

        [Bindable(true, BindingDirection.OneWay)] public FileSystemOptionsViewModel Options { get; }
        [Bindable(true, BindingDirection.OneWay)] public ReadOnlyObservableCollection<IFileSystemDrive> Items { get; }

        public FileSystemViewModel(IFileSystemViewModelFactory factory, FileSystemOptionsViewModel options)
        {
            ArgumentNullException.ThrowIfNull(factory);
            ArgumentNullException.ThrowIfNull(options);

            _items = new ObservableCollection<IFileSystemDrive>();
            Items = new ReadOnlyObservableCollection<IFileSystemDrive>(_items);
            Options = options;
            _mapper = new Mapper(this, factory);
        }

        [RelayCommand(AllowConcurrentExecutions = false)]
        private Task Refresh(CancellationToken token)
        {
            return _mapper.Refresh(token);
        }

        private sealed class Mapper : IViewModelMapper
        {
            private readonly FileSystemViewModel _viewModel;
            private readonly IFileSystemViewModelFactory _fileSystemViewModelFactory;

            public Mapper(
                FileSystemViewModel viewModel,
                IFileSystemViewModelFactory fileSystemViewModelFactory)
            {
                ArgumentNullException.ThrowIfNull(viewModel);
                ArgumentNullException.ThrowIfNull(fileSystemViewModelFactory);

                _viewModel = viewModel;
                _fileSystemViewModelFactory = fileSystemViewModelFactory;
            }

            public async Task Refresh(CancellationToken token)
            {
                var children = await _fileSystemViewModelFactory.GetDrives(_viewModel.Options.DriveTypes, _viewModel.Options.FileAttributes, _viewModel.Options.FolderAttributes, token);

                if (!_viewModel.IsLoaded)
                {
                    _viewModel._items.AddRange(children);
                    _viewModel.IsLoaded = true;
                }
            }
        }
    }
}
