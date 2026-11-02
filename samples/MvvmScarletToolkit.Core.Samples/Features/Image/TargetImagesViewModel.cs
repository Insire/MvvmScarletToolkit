using MvvmScarletToolkit.Observables;

namespace MvvmScarletToolkit.Core.Samples.Features.Image
{
    public sealed class TargetImagesViewModel : ViewModelListBase<ImageViewModel>
    {
        public TargetImagesViewModel(IScarletCommandBuilder commandBuilder)
            : base(commandBuilder)
        {
        }
    }
}
