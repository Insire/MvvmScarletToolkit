using MvvmScarletToolkit.Commands;
using MvvmScarletToolkit.Observables;
using System.Threading;
using System.Threading.Tasks;

namespace DemoApp
{
    public class DataContextSchenanigansViewModel : ViewModelListBase<AsyncDemoItem>
    {
        public DataContextSchenanigansViewModel(CommandBuilder commandBuilder)
            : base(commandBuilder)
        {
        }

        protected override async Task Load(CancellationToken token)
        {
            for (var i = 0; i < 10; i++)
            {
                await Add(new AsyncDemoItem(CommandBuilder)
                {
                    DisplayName = "Test X",
                }).ConfigureAwait(false);
            }

            IsLoaded = true;
        }

        protected override Task Refresh(CancellationToken token)
        {
            return Task.CompletedTask;
        }

        protected override async Task Unload(CancellationToken token)
        {
            await Clear().ConfigureAwait(false);
            IsLoaded = false;
        }
    }
}
