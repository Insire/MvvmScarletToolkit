using MvvmScarletToolkit.Observables;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Samples
{
    public class Images : BusinessViewModelListBase<Image>
    {
        public Images(IScarletCommandBuilder commandBuilder)
            : base(commandBuilder)
        {
        }

        protected override Task RefreshInternal(CancellationToken token)
        {
            return AddRange(ImageFactory.GetImageList());
        }
    }
}
