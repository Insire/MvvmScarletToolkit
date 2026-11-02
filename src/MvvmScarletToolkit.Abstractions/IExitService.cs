namespace MvvmScarletToolkit
{
    public interface IExitService
    {
        Task ShutDown();

        void UnloadOnExit(IVirtualizationViewModel viewModel);
    }
}
