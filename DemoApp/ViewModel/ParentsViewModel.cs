using MvvmScarletToolkit.Abstractions;
using MvvmScarletToolkit.Observables;

namespace DemoApp
{
    public class ParentsViewModel : ViewModelListBase<ParentViewModel>
    {
        public ParentsViewModel(ICommandBuilder commandBuilder)
            : base(commandBuilder)
        {
        }
    }
}
