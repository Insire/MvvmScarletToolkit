using MvvmScarletToolkit.Commands;
using MvvmScarletToolkit.Observables;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Wpf.Samples
{
    // parent observer and child at the same time
    public sealed class BusyViewModel : ViewModelListBase<INotifyPropertyChanged>, IObserver<bool>, IObservable<bool>, IDisposable
    {
        private readonly IDictionary<INotifyPropertyChanged, IDisposable> _disposables;

        public ConcurrentCommandBase AddContainerCommand { get; }
        public ConcurrentCommandBase AddChildCommand { get; }

        public BusyViewModel(IScarletCommandBuilder commandBuilder)
            : base(commandBuilder)
        {
            _disposables = new Dictionary<INotifyPropertyChanged, IDisposable>();

            AddChildCommand = CommandBuilder
                .Create(InternalAddChildAsync, CanAddChild)
                .WithBusyNotification(BusyStack)
                .WithSingleExecution()
                .Build();

            AddContainerCommand = CommandBuilder
                .Create(InternalAddContainerAsync, CanAddChild)
                .WithBusyNotification(BusyStack)
                .WithSingleExecution()
                .Build();
        }

        private async Task InternalAddContainerAsync(CancellationToken token)
        {
            var viewModel = new BusyViewModel(CommandBuilder);
            _disposables.Add(viewModel, viewModel.Subscribe(this));

            await Add(viewModel, token).ConfigureAwait(false);
            await Task.Delay(450, token).ConfigureAwait(false);
        }

        private async Task InternalAddChildAsync(CancellationToken token)
        {
            var viewModel = new ObservableBusyViewModel(CommandBuilder, Dispatcher);
            _disposables.Add(viewModel, viewModel.Subscribe(this));

            await Add(viewModel, token).ConfigureAwait(false);
            await Task.Delay(450, token).ConfigureAwait(false);
        }

        public override async Task Remove(INotifyPropertyChanged item, CancellationToken token)
        {
            _disposables[item].Dispose();
            await base.Remove(item, token).ConfigureAwait(false);
        }

        private bool CanAddChild()
        {
            return !IsBusy;
        }

        public void OnNext(bool value)
        {
            BusyStack.GetToken();
        }

        public async void OnError(Exception error)
        {
            await BusyStack.Pull().ConfigureAwait(false);
        }

        /// <summary>
        /// Unused
        /// </summary>
        public async void OnCompleted()
        {
            await BusyStack.Pull().ConfigureAwait(false);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var disposeable in _disposables)
                {
                    disposeable.Value.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        public IDisposable Subscribe(IObserver<bool> observer)
        {
            return BusyStack.Subscribe(observer);
        }
    }
}
