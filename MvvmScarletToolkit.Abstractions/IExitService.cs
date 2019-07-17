using System.Threading.Tasks;

namespace MvvmScarletToolkit.Abstractions
{
    public interface IExitService
    {
        Task ShutDown();

        void UnloadOnExit(IBusinessViewModelListBase viewModel);

        void UnloadOnExit(IBusinessViewModelBase viewModel);
    }
}
