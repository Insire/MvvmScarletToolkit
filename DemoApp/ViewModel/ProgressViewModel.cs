using MvvmScarletToolkit.Abstractions;
using MvvmScarletToolkit.Commands;
using MvvmScarletToolkit.Observables;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DemoApp
{
    public sealed class ProgressViewModel : ObservableObject
    {
        private readonly BusyStack _busyStack;

        private bool _isBusy;
        public bool IsBusy
        {
            get { return _isBusy; }
            private set { SetValue(ref _isBusy, value); }
        }

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

        private IAsyncCommand _loadCommand;
        public IAsyncCommand LoadCommand
        {
            get { return _loadCommand; }
            private set { SetValue(ref _loadCommand, value); }
        }

        public ProgressViewModel()
        {
            _loadCommand = AsyncCommand.Create(Load, CanLoad);

            _busyStack = new BusyStack(hasItems => IsBusy = hasItems);
            Maximum = 100;
        }

        private async Task Load(CancellationToken token)
        {
            using (_busyStack.GetToken())
            {
                await Task.Run(async () => await LoadInternal().ConfigureAwait(false), token).ConfigureAwait(false);
            }
        }

        private async Task LoadInternal()
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

        private bool CanLoad()
        {
            return !_isBusy;
        }
    }
}
