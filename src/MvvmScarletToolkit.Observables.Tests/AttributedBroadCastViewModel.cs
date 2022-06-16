using CommunityToolkit.Mvvm.ComponentModel;

namespace MvvmScarletToolkit.Observables.Tests
{
    [ObservableRecipient]
    internal sealed partial class AttributedBroadCastViewModel : ObservableObject, ITestViewModel
    {
        [ObservableProperty]
        [NotifyPropertyChangedRecipients]
        private string _property;
    }
}
