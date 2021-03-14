using MvvmScarletToolkit.Observables;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Wpf.Samples
{
    public sealed class ProgressViewModel : BusinessViewModelBase
    {
        private readonly DispatcherProgress<double> _progress;
        private readonly IProgress<double> _uiBlockingProgress;

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

        private bool _block;
        public bool Block
        {
            get { return _block; }
            set { SetProperty(ref _block, value); }
        }

        public ProgressViewModel(IScarletCommandBuilder commandBuilder)
            : base(commandBuilder)
        {
            Maximum = 100;
            _progress = new DispatcherProgress<double>(Dispatcher, killTheUIWithWork, TimeSpan.FromMilliseconds(50));
            _uiBlockingProgress = new Progress<double>(killTheUIWithWork);

            /// overwhelms the dispatcher and the UI thread with property changed notifications
            void killTheUIWithWork(double i) => Dispatcher.Invoke(() => Percentage = i).ConfigureAwait(false);
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

            var worker = Block ? new Worker(_uiBlockingProgress) : new Worker(_progress);

            await worker.DoWork().ConfigureAwait(false);
        }

        protected override void Dispose(bool disposing)
        {
            if (IsDisposed)
            {
                return;
            }

            if (disposing)
            {
                _progress.Dispose();
            }

            base.Dispose(disposing);
        }

        private sealed class Worker
        {
            private readonly IProgress<double> _progress;

            public Worker(IProgress<double> progress)
            {
                _progress = progress ?? throw new ArgumentNullException(nameof(progress));
            }

            public Task DoWork()
            {
                return Task.Run(() =>
                {
                    const double upperBound = 100_000_000d;

                    for (var i = 0d; i < upperBound; i++)
                    {
                        var percentage = i * 100 / upperBound;

                        _progress.Report(Math.Round(percentage, 0));
                    }
                });
            }
        }
    }
}
