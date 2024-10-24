using MvvmScarletToolkit.Observables;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Wpf.Samples.Features.Image
{
    public sealed class ImagesViewModel : BusinessViewModelListBase<ImageViewModel>
    {
        private readonly ImageViewModelProvider _provider;

        public ImagesViewModel(IScarletCommandBuilder commandBuilder, ImageViewModelProvider provider)
            : base(commandBuilder)
        {
            _provider = provider ?? throw new System.ArgumentNullException(nameof(provider));
        }

        protected override Task RefreshInternal(CancellationToken token)
        {
            return AddRange(_provider.GetImages(), token);
        }
    }
}
