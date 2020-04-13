using MvvmScarletToolkit.Commands;
using MvvmScarletToolkit.Observables;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Samples
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

            AddChildCommand = CommandBuilder.Create(InternalAddChildAsync, CanAddChild);
            AddContainerCommand = CommandBuilder.Create(InternalAddContainerAsync, CanAddChild);
        }

        private async Task InternalAddContainerAsync(CancellationToken token)
        {
            using (BusyStack.GetToken())
            {
                var viewModel = new BusyViewModel(CommandBuilder);
                _disposables.Add(viewModel, viewModel.Subscribe(this));

                await Add(viewModel).ConfigureAwait(false);
                await Task.Delay(450).ConfigureAwait(false);
            }
        }

        private async Task InternalAddChildAsync(CancellationToken token)
        {
            using (BusyStack.GetToken())
            {
                var viewModel = new ObservableBusyViewModel(CommandBuilder, Dispatcher);
                _disposables.Add(viewModel, viewModel.Subscribe(this));

                await Add(viewModel).ConfigureAwait(false);
                await Task.Delay(450).ConfigureAwait(false);
            }
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

        public async void OnNext(bool value)
        {
            if (value)
            {
                BusyStack.GetToken();
            }
            else
            {
                await BusyStack.Pull().ConfigureAwait(false);
            }
        }

        public async void OnError(Exception error)
        {
            await BusyStack.Pull().ConfigureAwait(false);
        }

        /// <summary>
        /// Unused
        /// </summary>
        public void OnCompleted()
        {
            //hm, what to do here? I guess nothing right now...
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
