using MvvmScarletToolkit.Abstractions;
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

        public IExtendedAsyncCommand AddContainerCommand { get; }
        public IExtendedAsyncCommand AddChildCommand { get; }

        public BusyViewModel(IScarletDispatcher dispatcher, ICommandManager commandManager)
            : base(dispatcher, commandManager)
        {
            _disposables = new Dictionary<ObservableObject, IDisposable>();

            AddChildCommand = AsyncCommand.Create(InternalAddChildAsync, CanAddChild, commandManager);
            AddContainerCommand = AsyncCommand.Create(InternalAddContainerAsync, CanAddChild, commandManager);
        }

        private async Task InternalAddContainerAsync(CancellationToken token)
        {
            using (BusyStack.GetToken())
            {
                var viewModel = new BusyViewModel(Dispatcher, CommandManager);
                _disposables.Add(viewModel, viewModel.Subscribe(this));

                await Add(viewModel).ConfigureAwait(false);
                await Task.Delay(450).ConfigureAwait(false);
            }
        }

        private async Task InternalAddChildAsync(CancellationToken token)
        {
            using (BusyStack.GetToken())
            {
                var viewModel = new ObservableBusyViewModel(CommandManager);
                _disposables.Add(viewModel, viewModel.Subscribe(this));

                await Add(viewModel).ConfigureAwait(false);
                await Task.Delay(450).ConfigureAwait(false);
            }
        }

        public override async Task Remove(ObservableObject item)
        {
            _disposables[item].Dispose();
            await base.Remove(item);
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
