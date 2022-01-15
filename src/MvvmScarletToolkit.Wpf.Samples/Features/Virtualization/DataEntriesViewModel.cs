using MvvmScarletToolkit.Observables;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MvvmScarletToolkit.Wpf.Samples
{
    public sealed class DataEntriesViewModel : BusinessSourceListViewModelBase<DataEntryViewModel>
    {
        public ICommand AddCommand { get; }

        public ICommand AddRangeCommand { get; }

        public DataEntriesViewModel(IScarletCommandBuilder commandBuilder, SynchronizationContext synchronizationContext)
            : base(commandBuilder, synchronizationContext, vm => vm.Id.ToString())
        {
            AddCommand = CommandBuilder.Create(AddNew, CanAddNew)
                .WithSingleExecution()
                .WithBusyNotification(BusyStack)
                .WithCancellation()
                .Build();

            AddRangeCommand = CommandBuilder
                .Create(AddRange, CanAddRange)
                .WithSingleExecution()
                .WithBusyNotification(BusyStack)
                .Build();
        }

        public Task AddNew()
        {
            return Add(new DataEntryViewModel(CommandBuilder));
        }

        public bool CanAddNew()
        {
            return Items != null;
        }

        public async Task AddRange()
        {
            using (BusyStack.GetToken())
            {
                await AddRange(Enumerable.Range(0, 5).Select(_ => new DataEntryViewModel(CommandBuilder))).ConfigureAwait(false);
            }
        }

        private bool CanAddRange()
        {
            return Items != null;
        }

        protected override Task RefreshInternal(CancellationToken token)
        {
            var viewModels = new DataEntryViewModel[1000];
            for (var i = 0; i < 1000; i++)
            {
                viewModels[i] = new DataEntryViewModel(CommandBuilder);
            }

            return AddRange(viewModels, token);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Items.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
