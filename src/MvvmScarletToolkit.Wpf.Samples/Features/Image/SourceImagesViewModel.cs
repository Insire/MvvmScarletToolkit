using MvvmScarletToolkit.Observables;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Wpf.Samples.Features.Image
{
    public sealed class SourceImagesViewModel : BusinessViewModelListBase<ImageViewModel>
    {
        private readonly ImageViewModelProvider _provider;
        private readonly TargetImagesViewModel _target;

        public SourceImagesViewModel(
            IScarletCommandBuilder commandBuilder,
            ImageViewModelProvider provider,
            TargetImagesViewModel target)
            : base(commandBuilder)
        {
            _provider = provider ?? throw new System.ArgumentNullException(nameof(provider));
            _target = target;
        }

        protected override Task RefreshInternal(CancellationToken token)
        {
            return AddRange(_provider.GetImages(this, _target), token);
        }
    }
}
