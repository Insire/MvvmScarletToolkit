using MvvmScarletToolkit.Observables;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MvvmScarletToolkit.Xamarin.Forms.Samples
{
    public sealed class MainPageViewModel : ViewModelBase
    {
        public ICommand WorkCommand { get; }
        public ICommand ResetCommand { get; }

        private int _count;
        public int Count
        {
            get { return _count; }
            set { SetValue(ref _count, value); }
        }

        public MainPageViewModel()
            : base(ScarletCommandBuilder.Default)
        {
            WorkCommand = CommandBuilder.Create(Work, CanWork)
                .WithSingleExecution()
                .WithBusyNotification(BusyStack)
                .Build();

            ResetCommand = CommandBuilder.Create(Reset, CanReset)
                .WithSingleExecution()
                .WithBusyNotification(BusyStack)
                .Build();
        }

        private async Task Reset(CancellationToken token)
        {
            Debug.WriteLine("Resetting ...");
            await Task.Delay(2000, token);
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
            await Task.Delay(2000, token);
            Debug.WriteLine("Done ...");

            Count++;
        }

        private bool CanWork()
        {
            Debug.WriteLine("CanWork?");
            return Count == 0;
        }
    }
}
