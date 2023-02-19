using CommunityToolkit.Mvvm.ComponentModel;

namespace MvvmScarletToolkit.Wpf.Samples
{
    public sealed partial class EnumViewModel : ObservableObject
    {
        [ObservableProperty]
        private ViewModelEnum _value;
    }
}
