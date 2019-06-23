using System.Threading.Tasks;

namespace MvvmScarletToolkit.Abstractions
{
    public interface IExitService
    {
        void UnloadOnExit(IBusinessViewModelListBase viewModel);
        void UnloadOnExit(IBusinessViewModelBase viewModel);
    }
}
