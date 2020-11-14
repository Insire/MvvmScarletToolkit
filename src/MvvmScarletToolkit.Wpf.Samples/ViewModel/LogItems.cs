using MvvmScarletToolkit.Observables;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MvvmScarletToolkit.Wpf.Samples
{
    public class LogItems : BusinessViewModelListBase<LogItem>
    {
        public ICommand AddCommand { get; }

        public LogItems(IScarletCommandBuilder commandBuilder)
            : base(commandBuilder)
        {
            AddCommand = CommandBuilder.Create(AddNew, CanAddNew)
                .WithSingleExecution()
                .WithBusyNotification(BusyStack)
                .WithCancellation()
                .Build();
        }

        public async Task AddNew()
        {
            await Add(new LogItem(CommandBuilder)).ConfigureAwait(false);
        }

        public bool CanAddNew()
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
