using CommunityToolkit.Mvvm.ComponentModel;

namespace MvvmScarletToolkit.Core.Samples.Features.Enums
{
    public sealed partial class EnumViewModel : ObservableObject
    {
        [ObservableProperty]
        public partial ViewModelEnum Value { get; set; }
    }
}
