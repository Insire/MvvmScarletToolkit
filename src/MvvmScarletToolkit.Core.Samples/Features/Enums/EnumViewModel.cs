using CommunityToolkit.Mvvm.ComponentModel;

namespace MvvmScarletToolkit.Core.Samples.Features.Enums
{
    public sealed partial class EnumViewModel : ObservableObject
    {
        [ObservableProperty]
        private ViewModelEnum _value;
    }
}
