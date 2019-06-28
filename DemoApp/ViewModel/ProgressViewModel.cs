using MvvmScarletToolkit;
using MvvmScarletToolkit.Abstractions;
using MvvmScarletToolkit.Observables;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace DemoApp
{
    public sealed class ProgressViewModel : BusinessViewModelBase
    {
        private double _percentage;
        public double Percentage
        {
            get { return _percentage; }
            private set { SetValue(ref _percentage, value); }
        }

        private double _maximum;
        public double Maximum
        {
            get { return _maximum; }
            private set { SetValue(ref _maximum, value); }
        }

        public ProgressViewModel(ICommandBuilder commandBuilder)
            : base(commandBuilder)
        {
            Maximum = 100;
        }

        protected override Task UnloadInternal(CancellationToken token)
        {
            Debug.WriteLine("Unloading ProgressViewModel");
            return Task.CompletedTask;
        }

        protected override async Task RefreshInternal(CancellationToken token)
        {
            if (!IsLoaded)
                Debug.WriteLine("Loading ProgressViewModel");
            else
                Debug.WriteLine("Refreshing ProgressViewModel");

            await Dispatcher.Invoke(() => Percentage = 0).ConfigureAwait(false);
            await Task.Delay(250).ConfigureAwait(false);

            for (var i = 0; i <= _maximum; i++)
            {
                if (token.IsCancellationRequested)
                    break;

                await Dispatcher.Invoke(() => Percentage = i, token).ConfigureAwait(false);
                await Task.Delay(25).ConfigureAwait(false);
            }
        }
    }
}
