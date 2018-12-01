using MvvmScarletToolkit;
using MvvmScarletToolkit.Observables;

namespace DemoApp
{
    public sealed class NavigationViewModel : Scenes
    {
        public NavigationViewModel()
            : base(new ScarletDispatcher())
        {
        }
    }
}
