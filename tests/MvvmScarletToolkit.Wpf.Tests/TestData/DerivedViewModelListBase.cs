using MvvmScarletToolkit.Observables;

namespace MvvmScarletToolkit.Tests
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
