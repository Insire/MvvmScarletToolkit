using MvvmScarletToolkit.Abstractions;
using MvvmScarletToolkit.Observables;

namespace DemoApp
{
    public sealed class NavigationViewModel : Scenes
    {
        public NavigationViewModel(ICommandBuilder commandBuilder)
            : base(commandBuilder)
        {
        }
    }
}
