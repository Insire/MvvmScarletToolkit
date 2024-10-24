using CommunityToolkit.Mvvm.ComponentModel;
using MvvmScarletToolkit.Observables;

namespace MvvmScarletToolkit.Wpf.Samples.Features.Image
{
    public sealed partial class ImageViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string? _displayName;

        [ObservableProperty]
        private string? _path;

        [ObservableProperty]
        private bool _isSelected;

        [ObservableProperty]
        private int _sequence;

        public ImageViewModel(IScarletCommandBuilder commandBuilder)
            : base(commandBuilder)
        {
        }
    }
}
