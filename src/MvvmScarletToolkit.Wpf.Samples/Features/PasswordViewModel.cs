using CommunityToolkit.Mvvm.ComponentModel;

namespace MvvmScarletToolkit.Wpf.Samples
{
    public sealed partial class PasswordViewModel : ObservableObject
    {
        [ObservableProperty]
        private string? _password;
    }
}
