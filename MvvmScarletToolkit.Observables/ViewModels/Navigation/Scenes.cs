using MvvmScarletToolkit.Abstractions;

namespace MvvmScarletToolkit.Observables
{
    public abstract class Scenes : ViewModelListBase<Scene>
    {
        protected Scenes(ICommandBuilder commandBuilder)
            : base(commandBuilder)
        {
        }
    }
}
