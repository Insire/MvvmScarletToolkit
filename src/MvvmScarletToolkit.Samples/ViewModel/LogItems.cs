using MvvmScarletToolkit.Observables;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MvvmScarletToolkit.Samples
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
            var item = new LogItem(CommandBuilder);
            await Add(item).ConfigureAwait(false);
        }

        public bool CanAddNew()
        {
            return Items != null;
        }

        protected override async Task RefreshInternal(CancellationToken token)
        {
            for (var i = 0; i < 1000; i++)
            {
                await AddNew();
            }
        }
    }
}
