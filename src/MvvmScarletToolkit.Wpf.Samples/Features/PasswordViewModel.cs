using CommunityToolkit.Mvvm.ComponentModel;

namespace MvvmScarletToolkit.Wpf.Samples
{
    [ObservableObject]
    public sealed partial class PasswordViewModel
    {
        [ObservableProperty]
        private string _password;
    }
}
