using MvvmScarletToolkit.Abstractions;
using MvvmScarletToolkit.Commands;

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
