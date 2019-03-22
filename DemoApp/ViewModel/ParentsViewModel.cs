using MvvmScarletToolkit.Commands;
using MvvmScarletToolkit.Observables;
using System.Threading;
using System.Threading.Tasks;

namespace DemoApp
{
    public class ParentsViewModel : ViewModelListBase<ParentViewModel>
    {
        public ParentsViewModel(CommandBuilder commandBuilder)
            : base(commandBuilder)
        {
        }

        protected override Task Load(CancellationToken token)
        {
            IsLoaded = true;
            return Task.CompletedTask;
        }

        protected override Task Refresh(CancellationToken token)
        {
            return Task.CompletedTask;
        }

        protected override async Task Unload(CancellationToken token)
        {
            await Clear(token).ConfigureAwait(false);
            IsLoaded = false;
        }
    }
}
