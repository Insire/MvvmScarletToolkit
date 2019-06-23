using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Abstractions
{
    public interface IBusinessViewModelBase
    {
        bool IsLoaded { get; }

        Task Load(CancellationToken token);
        Task Refresh(CancellationToken token);
        Task Unload(CancellationToken token);
    }
}
