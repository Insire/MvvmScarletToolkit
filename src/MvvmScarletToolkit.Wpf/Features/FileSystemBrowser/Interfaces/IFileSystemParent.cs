using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace MvvmScarletToolkit.Wpf.Features.FileSystemBrowser.Interfaces
{
    public interface IFileSystemParent : IFileSystemInfo
    {
        bool IsToggled { get; set; }
        bool IsEmpty { get; set; }
        bool HasChildContainers { get; set; }

        ReadOnlyObservableCollection<IFileSystemChild> Items { get; }

        IAsyncRelayCommand RefreshCommand { get; }
    }
}
