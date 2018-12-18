using MvvmScarletToolkit.Abstractions;
using MvvmScarletToolkit.Observables;
using System.Threading;
using System.Threading.Tasks;

namespace DemoApp
{
    public class Images : ViewModelListBase<Image>
    {
        public Images(IScarletDispatcher dispatcher)
            : base(dispatcher)
        {
        }

        protected override async Task LoadInternal(CancellationToken token)
        {
            await AddRange(ImageFactory.GetImageList()).ConfigureAwait(false);

            IsLoaded = true;
        }

        protected override Task UnloadInternalAsync()
        {
            Clear();
            IsLoaded = false;
            return Task.CompletedTask;
        }

        protected override Task RefreshInternal(CancellationToken token)
        {
            return Task.CompletedTask;
        }
    }
}
