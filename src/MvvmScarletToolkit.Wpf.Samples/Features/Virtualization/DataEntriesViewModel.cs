using MvvmScarletToolkit.Observables;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MvvmScarletToolkit.Wpf.Samples
{
    public sealed class DataEntriesViewModel : BusinessViewModelListBase<DataEntryViewModel>
    {
        public ICommand AddCommand { get; }

        public ICommand AddRangeCommand { get; }

        public DataEntriesViewModel(IScarletCommandBuilder commandBuilder)
            : base(commandBuilder)
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

        protected override async Task RefreshInternal(CancellationToken token)
        {
            for (var i = 0; i < 1000; i++)
            {
                await AddNew().ConfigureAwait(false);
            }
        }

        protected override async void Dispose(bool disposing)
        {
            if (disposing)
            {
                Items.Dispose();
                await Clear().ConfigureAwait(false);
            }

            base.Dispose(disposing);
        }
    }
}
