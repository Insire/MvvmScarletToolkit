using MvvmScarletToolkit.Commands;
using MvvmScarletToolkit.Observables;
using System.Threading;
using System.Threading.Tasks;

namespace DemoApp
{
    public class Images : ViewModelListBase<Image>
    {
        public Images(CommandBuilder commandBuilder)
            : base(commandBuilder)
        {
        }

        protected override async Task Load(CancellationToken token)
        {
            await AddRange(ImageFactory.GetImageList()).ConfigureAwait(false);

            IsLoaded = true;
        }

        protected override async Task Unload(CancellationToken token)
        {
            await Clear().ConfigureAwait(false);
            IsLoaded = false;
        }

        protected override Task Refresh(CancellationToken token)
        {
            return Task.CompletedTask;
        }
    }
}
