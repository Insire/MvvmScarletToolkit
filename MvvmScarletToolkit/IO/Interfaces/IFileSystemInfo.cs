using System.ComponentModel;
using System.Windows.Input;

namespace MvvmScarletToolkit
{
    public interface IFileSystemInfo : INotifyPropertyChanged
    {
        ICommand LoadCommand { get; }
        ICommand RefreshCommand { get; }

        string Name { get; }
        string FullName { get; }
        string Filter { get; set; }

        bool Exists { get; }
        bool IsSelected { get; }
        bool IsLoaded { get; }
        bool IsBusy { get; }
        bool IsHidden { get; }
        bool IsContainer { get; }

        bool HasContainers { get; }

        void Refresh();
        void LoadMetaData();
        void OnFilterChanged(string filter);
    }
}
