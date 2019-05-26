using MvvmScarletToolkit.Commands;
using MvvmScarletToolkit.Observables;

namespace DemoApp
{
    public sealed class NavigationViewModel : Scenes
    {
        public NavigationViewModel(CommandBuilder commandBuilder)
            : base(commandBuilder)
        {
        }
    }
}
