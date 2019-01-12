using MvvmScarletToolkit.Commands;
using MvvmScarletToolkit.Observables;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DemoApp
{
    // parent observer and child at the same time
    public sealed class BusyViewModel : ViewModelListBase<ObservableObject>, IObserver<bool>, IObservable<bool>, IDisposable
    {
        private readonly IDictionary<ObservableObject, IDisposable> _disposables;

        public ConcurrentCommandBase AddContainerCommand { get; }
        public ConcurrentCommandBase AddChildCommand { get; }

        public BusyViewModel(CommandBuilder commandBuilder)
            : base(commandBuilder)
        {
            _disposables = new Dictionary<ObservableObject, IDisposable>();

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
                var viewModel = new ObservableBusyViewModel(CommandBuilder);
                _disposables.Add(viewModel, viewModel.Subscribe(this));

                await Add(viewModel).ConfigureAwait(false);
                await Task.Delay(450).ConfigureAwait(false);
            }
        }

        public override async Task Remove(ObservableObject item)
        {
            _disposables[item].Dispose();
            await base.Remove(item).ConfigureAwait(false);
        }

        private bool CanAddChild()
        {
            return !IsBusy;
        }

        public void OnNext(bool value)
        {
            if (value)
            {
                BusyStack.GetToken();
            }
            else
            {
                BusyStack.Pull();
            }
        }

        public void OnError(Exception error)
        {
            BusyStack.Pull();
        }

        public void OnCompleted()
        {
            //hm, what to do here? I guess nothing right now...
        }

        public void Dispose()
        {
            foreach (var disposeable in _disposables)
            {
                disposeable.Value.Dispose();
            }
        }

        public IDisposable Subscribe(IObserver<bool> observer)
        {
            return BusyStack.Subscribe(observer);
        }

        protected override Task LoadInternal(CancellationToken token)
        {
            IsLoaded = true;
            return Task.CompletedTask;
        }

        protected override async Task UnloadInternalAsync()
        {
            await Clear().ConfigureAwait(false);
            IsLoaded = false;
        }

        protected override Task RefreshInternal(CancellationToken token)
        {
            return Task.CompletedTask;
        }
    }
}
