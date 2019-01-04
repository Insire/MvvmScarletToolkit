using MvvmScarletToolkit.Abstractions;
using MvvmScarletToolkit.Observables;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DemoApp
{
    public sealed class ProgressViewModel : ViewModelBase
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

        public ProgressViewModel(ICommandManager commandManager)
            : base(commandManager)
        {
            Maximum = 100;
        }

        protected override async Task LoadInternal(CancellationToken token)
        {
            using (BusyStack.GetToken())
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

            IsLoaded = true;
        }

        protected override bool CanLoad()
        {
            return !IsBusy && base.CanLoad();
        }

        protected override Task UnloadInternalAsync()
        {
            IsLoaded = false;
            return Task.CompletedTask;
        }

        protected override Task RefreshInternal(CancellationToken token)
        {
            return Task.CompletedTask;
        }
    }
}
