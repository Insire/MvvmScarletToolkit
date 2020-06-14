using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MvvmScarletToolkit
{
    public interface IVirtualizationViewModel : INotifyPropertyChanged
    {
        bool IsLoaded { get; }

        /// <summary>
        /// conditional refresh
        /// </summary>
        ICommand LoadCommand { get; }

        /// <summary>
        /// explicit refresh, updates filtering and the current children
        /// </summary>
        ICommand RefreshCommand { get; }

        ICommand UnloadCommand { get; }

        Task Load(CancellationToken token);

        Task Refresh(CancellationToken token);

        Task Unload(CancellationToken token);

        bool CanLoad();

        bool CanRefresh();

        bool CanUnload();
    }
}
