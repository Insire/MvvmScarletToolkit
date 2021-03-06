using MvvmScarletToolkit.Observables;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MvvmScarletToolkit.Wpf.Samples
{
    public class DialogViewModel : ViewModelBase
    {
        public ICommand RunCommand { get; }

        public BoolDialogResultViewModel DialogResult { get; }

        public DialogViewModel(IScarletCommandBuilder commandBuilder)
            : base(commandBuilder)
        {
            RunCommand = CommandBuilder.Create(Run, CanRun)
                .WithBusyNotification(BusyStack)
                .WithSingleExecution()
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
