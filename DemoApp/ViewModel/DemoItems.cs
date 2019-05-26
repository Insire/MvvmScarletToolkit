using MvvmScarletToolkit.Abstractions;
using MvvmScarletToolkit.Commands;
using MvvmScarletToolkit.Observables;
using System.Threading;
using System.Threading.Tasks;

namespace DemoApp
{
    public class DemoItems : BusinessViewModelListBase<DemoItem>
    {
        public ConcurrentCommandBase AddCommand { get; }

        public DemoItems(ICommandBuilder commandBuilder)
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

        protected override Task RefreshInternal(CancellationToken token)
        {
            IsLoaded = true;
            return Task.CompletedTask;
        }
    }
}
