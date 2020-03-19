using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MvvmScarletToolkit
{
    public interface IVirtualizationViewModel : INotifyPropertyChanged
    {
        bool IsLoaded { get; }

        ICommand LoadCommand { get; }
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
