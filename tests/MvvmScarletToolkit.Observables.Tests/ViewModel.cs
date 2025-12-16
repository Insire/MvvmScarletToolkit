using CommunityToolkit.Mvvm.ComponentModel;

namespace MvvmScarletToolkit.Observables.Tests
{
    internal sealed partial class ViewModel : ObservableRecipient
    {
        [ObservableProperty]
        [NotifyPropertyChangedRecipients]
        public partial string? Data { get; set; }
    }
}
