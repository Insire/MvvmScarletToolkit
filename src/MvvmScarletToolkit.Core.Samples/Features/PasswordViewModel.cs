using CommunityToolkit.Mvvm.ComponentModel;

namespace MvvmScarletToolkit.Core.Samples.Features
{
    public sealed partial class PasswordViewModel : ObservableObject
    {
        [ObservableProperty]
        private string? _password;
    }
}
