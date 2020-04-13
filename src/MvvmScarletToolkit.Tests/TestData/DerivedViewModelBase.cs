using MvvmScarletToolkit.Observables;
using System;

namespace MvvmScarletToolkit.Tests
{
    internal sealed class DerivedViewModelBase : ViewModelBase
    {
        public DerivedViewModelBase(IScarletCommandBuilder commandBuilder)
            : base(commandBuilder)
        {
        }

        public void SetIsBusy(bool value)
        {
            IsBusy = value;
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
