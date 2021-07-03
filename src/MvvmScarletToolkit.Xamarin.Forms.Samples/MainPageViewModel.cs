using MvvmScarletToolkit.Commands;
using MvvmScarletToolkit.Observables;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Xamarin.Forms.Samples
{
    public sealed class MainPageViewModel : ViewModelBase
    {
        public ConcurrentCommandBase WorkCommand { get; }
        public ConcurrentCommandBase ResetCommand { get; }

        private int _count;
        public int Count
        {
            get { return _count; }
            set { SetProperty(ref _count, value); }
        }

        public MainPageViewModel()
            : base(ScarletCommandBuilder.Default)
        {
            WorkCommand = CommandBuilder
                .Create(Work, CanWork)
                .WithSingleExecution("Work")
                .WithBusyNotification(BusyStack)
                .WithAsyncCancellation()
                .Build();

            ResetCommand = CommandBuilder
                .Create(Reset, CanReset)
                .WithSingleExecution("Reset")
                .WithBusyNotification(BusyStack)
                .Build();
        }

        private async Task Reset(CancellationToken token)
        {
            Debug.WriteLine("Resetting ...");
            await Task.Delay(2000, token).ConfigureAwait(false);
            Debug.WriteLine("Reset complete ...");

            Count = 0;
        }

        private bool CanReset()
        {
            Debug.WriteLine("CanReset?");
            return Count > 0;
        }

        private async Task Work(CancellationToken token)
        {
            Debug.WriteLine("Working ...");
            await Task.Delay(2000, token).ConfigureAwait(false);
            Debug.WriteLine("Done ...");

            token.ThrowIfCancellationRequested();

            Count++;
        }

        private bool CanWork()
        {
            Debug.WriteLine("CanWork?");
            return Count == 0;
        }
    }
}
