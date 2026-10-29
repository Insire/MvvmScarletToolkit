using MvvmScarletToolkit.Observables;

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

        protected override async Task RefreshInternal(CancellationToken token)
        {
            await _provider.Initialize(token).ConfigureAwait(false);
            await AddRange(_provider.GetImages(this, _target), token).ConfigureAwait(false);
        }
    }
}
