using MvvmScarletToolkit.Abstractions;
using MvvmScarletToolkit.Commands;
using MvvmScarletToolkit.Observables;
using System.Threading;
using System.Threading.Tasks;

namespace DemoApp
{
    public class DemoItems : ViewModelListBase<DemoItem>
    {
        public IExtendedAsyncCommand AddCommand { get; }

        public DemoItems(IScarletDispatcher dispatcher)
            : base(dispatcher)
        {
            AddCommand = AsyncCommand.Create(AddNew, CanAddNew);
        }

        public async Task AddNew()
        {
            var item = new DemoItem();
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
