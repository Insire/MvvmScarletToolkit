using System.Collections.ObjectModel;

namespace MvvmScarletToolkit.Wpf.Features.FileSystemBrowser.Interfaces
{
    public interface IFileSystemInfo
    {
        ReadOnlyObservableCollection<PropertyViewModel> Properties { get; }

        string Name { get; }

        FileSystemType FileSystemType { get; }

        string FullName { get; }

        bool IsAccessProhibited { get; }

        bool IsSelected { get; set; }

        bool IsContainer { get; }

        bool IsLoaded { get; }

        bool Exists { get; }

        bool IsBusy { get; }
    }
}
