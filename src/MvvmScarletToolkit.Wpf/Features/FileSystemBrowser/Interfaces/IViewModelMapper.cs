using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Wpf.Features.FileSystemBrowser.Interfaces
{
    public interface IViewModelMapper
    {
        Task Refresh(CancellationToken token);
    }
}
