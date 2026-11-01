using CommunityToolkit.Mvvm.ComponentModel;

namespace MvvmScarletToolkit.Wpf.Samples.Features
{
    public sealed partial class SelectableTextViewModel : ObservableObject
    {
        [ObservableProperty]
        public partial string Message { get; set; } = "This text is bound. Changing it in the box below clears the selection.";
    }
}
