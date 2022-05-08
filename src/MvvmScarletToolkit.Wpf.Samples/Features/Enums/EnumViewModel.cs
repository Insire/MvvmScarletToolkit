using CommunityToolkit.Mvvm.ComponentModel;

namespace MvvmScarletToolkit.Wpf.Samples
{
    [ObservableObject]
    public sealed partial class EnumViewModel
    {
        [ObservableProperty]
        private ViewModelEnum _value;
    }
}
