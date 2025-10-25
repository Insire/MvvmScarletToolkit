using MvvmScarletToolkit.Observables;

namespace MvvmScarletToolkit.Wpf.Samples.Features.Image
{
    public sealed class TargetImagesViewModel : ViewModelListBase<ImageViewModel>
    {
        public TargetImagesViewModel(IScarletCommandBuilder commandBuilder)
            : base(commandBuilder)
        {
        }
    }
}
