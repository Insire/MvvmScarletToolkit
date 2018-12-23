using MvvmScarletToolkit.Abstractions;
using MvvmScarletToolkit.Observables;
using System.Threading;
using System.Threading.Tasks;

namespace DemoApp
{
    public class DataContextSchenanigansViewModel : ViewModelListBase<AsyncDemoItem>
    {
        public DataContextSchenanigansViewModel(IScarletDispatcher dispatcher)
            : base(dispatcher)
        {
        }

        protected override async Task LoadInternal(CancellationToken token)
        {
            for (var i = 0; i < 10; i++)
            {
                await Add(new AsyncDemoItem
                {
                    DisplayName = "Test X",
                }).ConfigureAwait(false);
            }

            IsLoaded = true;
        }

        protected override Task RefreshInternal(CancellationToken token)
        {
            return Task.CompletedTask;
        }

        protected override async Task UnloadInternalAsync()
        {
            await Clear().ConfigureAwait(false);
            IsLoaded = false;
        }
    }
}
