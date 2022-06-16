using CommunityToolkit.Mvvm.ComponentModel;

namespace MvvmScarletToolkit.Wpf.Samples
{
    [INotifyPropertyChanged]
    public sealed partial class EnumViewModel
    {
        [ObservableProperty]
        private ViewModelEnum _value;
    }
}
