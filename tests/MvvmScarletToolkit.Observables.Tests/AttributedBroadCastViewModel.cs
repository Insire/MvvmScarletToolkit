using CommunityToolkit.Mvvm.ComponentModel;

namespace MvvmScarletToolkit.Observables.Tests
{
    [ObservableRecipient]
    internal sealed partial class AttributedBroadCastViewModel : ObservableObject, ITestViewModel
    {
        [ObservableProperty]
        [NotifyPropertyChangedRecipients]
        public partial string? Property { get; set; }
    }
}
