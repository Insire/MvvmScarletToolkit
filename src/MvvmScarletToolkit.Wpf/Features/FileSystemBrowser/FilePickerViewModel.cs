using CommunityToolkit.Mvvm.ComponentModel;
using MvvmScarletToolkit.Wpf.Features.FileSystemBrowser.Drives;
using MvvmScarletToolkit.Wpf.Features.FileSystemBrowser.Files;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace MvvmScarletToolkit.Wpf.Features.FileSystemBrowser
{
    public class FilePickerViewModel : ObservableObject
    {
        private readonly FileSystemViewModel _fileSystemViewModel;

        public FilePickerViewModel(FileSystemViewModel fileSystemViewModel)
        {
            _fileSystemViewModel = fileSystemViewModel;
        }

        [Bindable(true, BindingDirection.OneWay)] public ReadOnlyObservableCollection<IFileSystemDrive> Items => _fileSystemViewModel.Items;

        [Bindable(true, BindingDirection.OneWay)] public ScarletFile? SelectedFile => _fileSystemViewModel.SelectedFile;
    }
}
