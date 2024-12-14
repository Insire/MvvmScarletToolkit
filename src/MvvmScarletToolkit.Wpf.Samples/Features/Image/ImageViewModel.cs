using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MvvmScarletToolkit.Observables;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Wpf.Samples.Features.Image
{
    public sealed partial class ImageViewModel : ViewModelBase
    {
        private readonly TargetImagesViewModel _target;
        private readonly SourceImagesViewModel _source;

        [ObservableProperty]
        private string? _displayName;

        [ObservableProperty]
        private string? _path;

        [ObservableProperty]
        private bool _isSelected;

        [ObservableProperty]
        private int _sequence;

        public ImageViewModel(IScarletCommandBuilder commandBuilder, TargetImagesViewModel target, SourceImagesViewModel source)
            : base(commandBuilder)
        {
            _target = target;
            _source = source;
        }

        [RelayCommand(CanExecute = nameof(CanMoveToTarget), AllowConcurrentExecutions = false)]
        private async Task MoveToTarget(object? element)
        {
            await _source.Remove(this).ConfigureAwait(false);
            if (!_target.Items.Contains(this))
            {
                await _target.Add(this).ConfigureAwait(false);
            }
        }

        private bool CanMoveToTarget(object? element)
        {
            return true;
        }
    }
}
