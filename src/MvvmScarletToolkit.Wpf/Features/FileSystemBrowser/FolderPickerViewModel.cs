using CommunityToolkit.Mvvm.ComponentModel;
using MvvmScarletToolkit.Wpf.Features.FileSystemBrowser.Drives;
using MvvmScarletToolkit.Wpf.Features.FileSystemBrowser.Files;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace MvvmScarletToolkit.Wpf.Features.FileSystemBrowser
{
    public class FolderPickerViewModel : ObservableObject
    {
        public FolderPickerViewModel(FileSystemViewModel fileSystemViewModel)
        {
            FileSystem = fileSystemViewModel;
        }

        [Bindable(true, BindingDirection.OneWay)] public ReadOnlyObservableCollection<IFileSystemDrive> Items => FileSystem.Items;

        [Bindable(true, BindingDirection.OneWay)] public ScarletFile? SelectedFile => FileSystem.SelectedFile;

        [Bindable(true, BindingDirection.OneWay)] public FileSystemViewModel FileSystem { get; }
    }
}
