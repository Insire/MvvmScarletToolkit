using MvvmScarletToolkit;
using MvvmScarletToolkit.Observables;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DemoApp
{
    public class VirtualizationViewModel : ViewModelBase
    {
        public ICommand AddRangeCommand { get; }

        private LogItems _logItems;
        public LogItems LogItems
        {
            get { return _logItems; }
            private set { SetValue(ref _logItems, value); }
        }

        public VirtualizationViewModel(LogItems logItems, ICommandBuilder commandBuilder)
            : base(commandBuilder)
        {
            LogItems = logItems ?? throw new ArgumentNullException(nameof(logItems));
            AddRangeCommand = CommandBuilder
                .Create(AddRange, CanAddRange)
                .WithSingleExecution(CommandManager)
                .WithBusyNotification(BusyStack)
                .Build();
        }

        public async Task AddRange()
        {
            using (BusyStack.GetToken())
            {
                await LogItems.AddRange(Enumerable.Range(0, 5).Select(p => new LogItem(CommandBuilder))).ConfigureAwait(false);
            }
        }

        private bool CanAddRange()
        {
            return LogItems.Items != null;
        }
    }
}
