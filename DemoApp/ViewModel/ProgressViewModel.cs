using MvvmScarletToolkit;
using MvvmScarletToolkit.Abstractions;
using MvvmScarletToolkit.Observables;
using System;
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

            using (var progress = new DispatcherProgress<int>(Dispatcher, (i) => Dispatcher.Invoke(() => Percentage = i, token).ConfigureAwait(false)))
            {
                var worker = new Worker(progress);

                await worker.DoWork().ConfigureAwait(false);
            }
        }
    }

    public sealed class Worker
    {
        private readonly IProgress<int> _progress;

        public Worker(IProgress<int> progress)
        {
            _progress = progress;
        }

        public Task DoWork()
        {
            return Task.Run(async () =>
            {
                var rnd = new Random(64782);
                var current = 0;
                var percentage = 0;

                do
                {
                    current = rnd.Next(current, 64782);
                    var d = current * 100 / 64782d;
                    percentage = (int)Math.Round(d, MidpointRounding.ToEven);

                    await Task.Delay(percentage * 10);
                    _progress.Report(percentage);
                } while (percentage < 100);
            });
        }
    }
}
