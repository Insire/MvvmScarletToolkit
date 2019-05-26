using MvvmScarletToolkit.Commands;
using MvvmScarletToolkit.Observables;
using System.Threading;
using System.Threading.Tasks;

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
