using MvvmScarletToolkit.Observables;

namespace MvvmScarletToolkit.Wpf.Samples
{
    public sealed class PasswordViewModel : ObservableObject
    {
        private string _password;
        public string Password
        {
            get { return _password; }
            set { SetValue(ref _password, value); }
        }
    }
}
