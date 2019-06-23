using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MvvmScarletToolkit.Abstractions
{
    public interface IBusinessViewModelListBase
    {
        ICommand ClearCommand { get; }
        bool IsLoaded { get; }
        ICommand LoadCommand { get; }

        Task Refresh(CancellationToken token);

        Task Unload(CancellationToken token);
    }
}
