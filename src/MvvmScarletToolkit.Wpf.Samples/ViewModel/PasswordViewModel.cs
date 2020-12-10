using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace MvvmScarletToolkit.Wpf.Samples
{
    public sealed class PasswordViewModel : ObservableObject
    {
        private string _password;
        public string Password
        {
            get { return _password; }
            set { SetProperty(ref _password, value); }
        }
    }
}
