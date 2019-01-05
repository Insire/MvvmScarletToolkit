using MvvmScarletToolkit.Commands;
using MvvmScarletToolkit.Observables;
using System.Threading;
using System.Threading.Tasks;

namespace DemoApp
{
    public sealed class NavigationViewModel : Scenes
    {
        public NavigationViewModel(CommandBuilder commandBuilder)
            : base(commandBuilder)
        {
        }

        protected override Task LoadInternal(CancellationToken token)
        {
            IsLoaded = true;
            return Task.CompletedTask;
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
