using MvvmScarletToolkit.Observables;

namespace MvvmScarletToolkit.Tests
{
    internal sealed class DerivedViewModelBase : ViewModelBase
    {
        public DerivedViewModelBase(IScarletCommandBuilder commandBuilder)
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
