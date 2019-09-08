using MvvmScarletToolkit.Observables;
using System;
using System.Collections.Generic;
using System.Text;

namespace DemoApp
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
