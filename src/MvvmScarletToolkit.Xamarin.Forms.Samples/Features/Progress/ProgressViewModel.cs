using MvvmScarletToolkit.Commands;
using MvvmScarletToolkit.Observables;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Xamarin.Forms.Samples
{
    public sealed class ProgressViewModel : ViewModelBase
    {
        private readonly DispatcherProgress<double> _progress;

        private double _percentage;
        public double Percentage
        {
            get { return _percentage; }
            private set { SetProperty(ref _percentage, value); }
        }

        private double _maximum;
        public double Maximum
        {
            get { return _maximum; }
            private set { SetProperty(ref _maximum, value); }
        }

        public ConcurrentCommandBase ProgressCommand { get; }

        public ProgressViewModel()
            : base(ScarletCommandBuilder.Default)
        {
            Maximum = 100;
            _progress = new DispatcherProgress<double>(Dispatcher, SetPercentage, TimeSpan.FromMilliseconds(50));

            ProgressCommand = CommandBuilder
                .Create(DoWorkImpl, () => ProgressCommand.IsNotBusy)
                .WithSingleExecution(nameof(ProgressCommand))
                .WithCancellation()
                .Build();
        }

        private async Task DoWorkImpl(CancellationToken token)
        {
            await Dispatcher.Invoke(() => Percentage = 0).ConfigureAwait(false);
            await Task.Delay(250, token).ConfigureAwait(false);

            await Task.Run(() =>
            {
                const double upperBound = 100_000_000d;

                for (var i = 0d; i < upperBound; i++)
                {
                    var percentage = i * 100 / upperBound;

                    _progress.Report(Math.Round(percentage, 0));
                }
            }).ConfigureAwait(false);
        }

        private void SetPercentage(double percentage)
        {
            Percentage = percentage;
        }
    }
}
