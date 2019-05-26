using MvvmScarletToolkit.Commands;
using MvvmScarletToolkit.Observables;
using System;
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

        public ProgressViewModel(CommandBuilder commandBuilder)
            : base(commandBuilder)
        {
            Maximum = 100;
        }

        protected override Task UnloadInternal(CancellationToken token)
        {
            return Task.CompletedTask;
        }

        protected override async Task RefreshInternal(CancellationToken token)
        {
            var dispatcher = System.Windows.Application.Current.Dispatcher;

            await dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() => Percentage = 0));
            await Task.Delay(250).ConfigureAwait(false);

            for (var i = 0; i <= _maximum; i++)
            {
                await dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() => Percentage = i));
                await Task.Delay(50).ConfigureAwait(false);
            }
        }
    }
}
