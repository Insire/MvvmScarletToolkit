using MvvmScarletToolkit.Commands;
using MvvmScarletToolkit.Observables;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Xamarin.Forms.Samples
{
    public sealed class StatefulCommandViewModel : ViewModelBase
    {
        public string Description { get; } = "This Implementation provides a command that can be run once at a time and can provide feedback to its current execution state. The CommandButton will be disabled during execution and after that until its being reset.";

        public ConcurrentCommandBase Command { get; }
        public ConcurrentCommandBase ResetCommand { get; }

        private int _count;
        public int Count
        {
            get { return _count; }
            private set { SetProperty(ref _count, value); }
        }

        public StatefulCommandViewModel()
            : base(ScarletCommandBuilder.Default)
        {
            Command = CommandBuilder
                .Create(CommandImpl, CanImpl)
                .WithSingleExecution("RegularCommand")
                .WithBusyNotification(BusyStack)
                .Build();

            ResetCommand = CommandBuilder
                 .Create(Reset, CanReset)
                 .WithSingleExecution("ResetCommand")
                 .WithBusyNotification(BusyStack)
                 .Build();
        }

        private async Task CommandImpl(CancellationToken token)
        {
            await Task.Delay(2000, token).ConfigureAwait(false);

            Count++;
        }

        private bool CanImpl()
        {
            return !Command.IsBusy && Count == 0;
        }

        private async Task Reset(CancellationToken token)
        {
            await Task.Delay(2000, token).ConfigureAwait(false);

            Count = 0;
        }

        private bool CanReset()
        {
            return !ResetCommand.IsBusy && Count > 0;
        }
    }
}
