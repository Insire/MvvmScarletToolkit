using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MvvmScarletToolkit.Observables;

namespace MvvmScarletToolkit.Wpf.Samples.Features.Image
{
    public sealed partial class ImageViewModel : ViewModelBase
    {
        private readonly TargetImagesViewModel _target;
        private readonly SourceImagesViewModel _source;

        [ObservableProperty] public partial string? DisplayName { get; set; }
        [ObservableProperty] public partial string? Path { get; set; }
        [ObservableProperty] public partial bool IsSelected { get; set; }
        [ObservableProperty] public partial int Sequence { get; set; }

        public ImageViewModel(IScarletCommandBuilder commandBuilder, TargetImagesViewModel target, SourceImagesViewModel source)
            : base(commandBuilder)
        {
            _target = target;
            _source = source;
        }

        [RelayCommand(CanExecute = nameof(CanMoveToTarget), AllowConcurrentExecutions = false)]
        private async Task MoveToTarget(object? element)
        {
            // no ConfigureAwait(false), Items may only be read on the UI thread, which modifies it
            await _source.Remove(this);
            if (!_target.Items.Contains(this))
            {
                await _target.Add(this);
            }
        }

        private static bool CanMoveToTarget(object? element)
        {
            return true;
        }
    }
}
