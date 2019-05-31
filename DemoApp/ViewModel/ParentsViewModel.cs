using MvvmScarletToolkit.Commands;
using MvvmScarletToolkit.Observables;

namespace DemoApp
{
    public class ParentsViewModel : ViewModelListBase<ParentViewModel>
    {
        public ParentsViewModel(CommandBuilder commandBuilder)
            : base(commandBuilder)
        {
        }
    }
}
