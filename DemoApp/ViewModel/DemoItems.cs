using MvvmScarletToolkit.Commands;
using MvvmScarletToolkit.Observables;
using System.Threading;
using System.Threading.Tasks;

namespace DemoApp
{
    public class DemoItems : ViewModelListBase<DemoItem>
    {
        public ConcurrentCommandBase AddCommand { get; }

        public DemoItems(CommandBuilder commandBuilder)
            : base(commandBuilder)
        {
            AddCommand = CommandBuilder.Create(AddNew, CanAddNew);
        }

        public async Task AddNew()
        {
            var item = new DemoItem(CommandBuilder);
            await Add(item).ConfigureAwait(false);
        }

        public bool CanAddNew()
        {
            return Items != null;
        }

        protected override Task LoadInternal(CancellationToken token)
        {
            IsLoaded = true;
            return Task.CompletedTask;
        }

        protected override async Task UnloadInternalAsync()
        {
            await Clear().ConfigureAwait(false);
            IsLoaded = false;
        }

        protected override Task RefreshInternal(CancellationToken token)
        {
            return Task.CompletedTask;
        }
    }
}
