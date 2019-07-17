using MvvmScarletToolkit;
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
