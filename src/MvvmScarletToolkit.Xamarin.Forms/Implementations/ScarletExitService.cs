using System;
using System.Threading.Tasks;

namespace MvvmScarletToolkit
{
    /// <summary>
    /// currently does nothing, until i can figure out how xamarin forms does handle app shutdowns - if at all
    /// </summary>
    public sealed class ScarletExitService : IExitService
    {
        private static readonly Lazy<ScarletExitService> _default = new Lazy<ScarletExitService>(() => new ScarletExitService());

        public static IExitService Default => _default.Value;

        public ScarletExitService()
        {
        }

        public Task ShutDown()
        {
            return Task.CompletedTask;
        }

        public void UnloadOnExit(IVirtualizationViewModel viewModel)
        {
        }
    }
}
