using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Wpf.FileSystemBrowser
{
    public interface IViewModelMapper
    {
        Task Refresh(CancellationToken token);
    }
}
