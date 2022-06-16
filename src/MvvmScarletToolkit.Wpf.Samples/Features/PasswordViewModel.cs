using CommunityToolkit.Mvvm.ComponentModel;

namespace MvvmScarletToolkit.Wpf.Samples
{
    [INotifyPropertyChanged]
    public sealed partial class PasswordViewModel
    {
        [ObservableProperty]
        private string _password;
    }
}
