using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

namespace MvvmScarletToolkit.Observables.Tests
{
    internal sealed partial class BroadCastViewModel : ObservableRecipient, ITestViewModel
    {
        private string property;
        public string Property
        {
            get { return property; }
            set { SetProperty(ref property, value, true); }
        }

        public BroadCastViewModel(IMessenger messenger)
            : base(messenger)
        {
        }
    }
}
