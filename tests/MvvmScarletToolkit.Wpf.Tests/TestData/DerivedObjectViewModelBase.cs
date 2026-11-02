using MvvmScarletToolkit.Observables;

namespace MvvmScarletToolkit.Wpf.Tests.TestData
{
    internal sealed class DerivedObjectViewModelBase : ViewModelBase<object>
    {
        public DerivedObjectViewModelBase(IScarletCommandBuilder commandBuilder, object? model)
            : base(commandBuilder, model)
        {
        }
    }
}
