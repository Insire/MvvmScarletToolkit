using MvvmScarletToolkit.Commands;
using MvvmScarletToolkit.Observables;

namespace MvvmScarletToolkit.Core.Samples.Features.Dialog
{
    public class DialogViewModel : ViewModelBase
    {
        public ConcurrentCommandBase RunCommand { get; }

        public BoolDialogResultViewModel DialogResult { get; }

        public DialogViewModel(IScarletCommandBuilder commandBuilder)
            : base(commandBuilder)
        {
            RunCommand = CommandBuilder.Create(Run, CanRun)
                .WithDisabledReason(() => "Wait until the current operation finishes")
                .WithBusyNotification(BusyStack)
                .WithSingleExecution(nameof(RunCommand), "The dialog is already open")
                .WithAsyncCancellation()
                .Build();

            DialogResult = new BoolDialogResultViewModel(commandBuilder);
        }

        private async Task Run(CancellationToken token)
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, token);
            await DialogResult.Show(linked.Token).ConfigureAwait(false);
        }

        private bool CanRun()
        {
            return !IsBusy;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DialogResult.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
