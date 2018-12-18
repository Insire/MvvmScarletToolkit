using MvvmScarletToolkit.Abstractions;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.FileSystemBrowser
{
    public interface IFileSystemInfo : INotifyPropertyChanged
    {
        /// <summary>
        /// conditional refresh
        /// </summary>
        IExtendedAsyncCommand LoadCommand { get; }

        IExtendedAsyncCommand UnloadCommand { get; }
        /// <summary>
        /// explicit refresh, updates filtering and the current children
        /// </summary>
        IExtendedAsyncCommand RefreshCommand { get; }

        /// <summary>
        /// delete an object from the filesystem
        /// </summary>
        IExtendedAsyncCommand DeleteCommand { get; }

        IExtendedAsyncCommand ToggleExpandCommand { get; }

        /// <summary>
        /// the logical parent
        /// </summary>
        IFileSystemDirectory Parent { get; }

        string Name { get; }
        string FullName { get; }
        string Filter { get; set; }

        bool IsExpanded { get; }
        bool IsSelected { get; set; }

        bool Exists { get; }
        bool IsLoaded { get; }
        bool IsBusy { get; }
        bool IsHidden { get; }
        bool IsContainer { get; }

        bool HasContainers { get; }

        /// <summary>
        /// Loads attributes and checks if the object still exists on file system
        /// </summary>
        Task LoadMetaData(CancellationToken token);

        /// <summary>
        /// updates the filtering
        /// </summary>
        /// <param name="filter"></param>
        Task OnFilterChanged(string filter, CancellationToken token);

        Task Delete(CancellationToken token);

        bool CanDelete();
    }
}
