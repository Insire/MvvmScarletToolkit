using MvvmScarletToolkit.Observables;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Samples
{
    public class Images : BusinessViewModelListBase<Image>
    {
        private readonly ImageFactory _imageFactory;

        public Images(IScarletCommandBuilder commandBuilder, ImageFactory imageFactory)
            : base(commandBuilder)
        {
            _imageFactory = imageFactory ?? throw new System.ArgumentNullException(nameof(imageFactory));
        }

        protected override Task RefreshInternal(CancellationToken token)
        {
            return AddRange(_imageFactory.GetImageList());
        }
    }
}
