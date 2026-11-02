using CommunityToolkit.Mvvm.ComponentModel;

namespace MvvmScarletToolkit.Core.Samples.Features
{
    public sealed partial class PasswordViewModel : ObservableObject
    {
        [ObservableProperty]
        public partial string? Password { get; set; }
    }
}
