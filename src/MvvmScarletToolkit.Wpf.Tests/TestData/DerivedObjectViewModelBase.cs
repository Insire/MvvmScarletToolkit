using MvvmScarletToolkit.Observables;

namespace MvvmScarletToolkit.Tests
{
    internal sealed class DerivedObjectViewModelBase : ViewModelBase<object>
    {
        public DerivedObjectViewModelBase(IScarletCommandBuilder commandBuilder, object model)
            : base(commandBuilder, model)
        {
        }
    }
}
