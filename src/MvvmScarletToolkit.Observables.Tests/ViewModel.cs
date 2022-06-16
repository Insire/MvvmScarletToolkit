using CommunityToolkit.Mvvm.ComponentModel;

namespace MvvmScarletToolkit.Observables.Tests
{
    [ObservableObject]
    [ObservableRecipient]
    internal sealed partial class ViewModel
    {
        [ObservableProperty]
        [NotifyPropertyChangedRecipients]
        private string _data;
    }
}
