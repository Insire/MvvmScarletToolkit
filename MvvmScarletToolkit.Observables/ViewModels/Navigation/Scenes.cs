using MvvmScarletToolkit.Commands;

namespace MvvmScarletToolkit.Observables
{
    public abstract class Scenes : ViewModelListBase<Scene>
    {
        protected Scenes(CommandBuilder commandBuilder)
            : base(commandBuilder)
        {
        }
    }
}
