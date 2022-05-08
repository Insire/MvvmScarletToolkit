using CommunityToolkit.Mvvm.ComponentModel;

namespace MvvmScarletToolkit.Observables.Tests
{
    [ObservableObject]
    internal sealed partial class ViewModel
    {
        [ObservableProperty]
        private object _property;
    }
}
