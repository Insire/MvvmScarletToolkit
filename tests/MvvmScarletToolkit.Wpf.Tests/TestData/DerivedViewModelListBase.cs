using MvvmScarletToolkit.Observables;

namespace MvvmScarletToolkit.Wpf.Tests.TestData
{
    internal sealed class DerivedViewModelListBase : ViewModelListBase<DerivedObjectViewModelBase>
    {
        public DerivedViewModelListBase(IScarletCommandBuilder commandBuilder)
            : base(commandBuilder)
        {
        }

        public void ValidateState(Action action)
        {
            using (BusyStack.GetToken())
            {
                action?.Invoke();
            }
        }
    }
}
