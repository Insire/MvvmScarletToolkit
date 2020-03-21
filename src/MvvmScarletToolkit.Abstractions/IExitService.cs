using System.Threading.Tasks;

namespace MvvmScarletToolkit
{
    public interface IExitService
    {
        Task ShutDown();

        void UnloadOnExit(IVirtualizationViewModel viewModel);
    }
}
