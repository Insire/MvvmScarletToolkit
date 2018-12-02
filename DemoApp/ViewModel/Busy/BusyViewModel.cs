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

        public BusyViewModel(IScarletDispatcher dispatcher)
            : base(dispatcher)
        {
            _disposables = new Dictionary<ObservableObject, IDisposable>();

            AddChildCommand = AsyncCommand.Create(InternalAddChildAsync, CanAddChild);
            AddContainerCommand = AsyncCommand.Create(InternalAddContainerAsync, CanAddChild);
        }

        private async Task InternalAddContainerAsync(CancellationToken token)
        {
            using (BusyStack.GetToken())
            {
                var viewModel = new BusyViewModel(Dispatcher);
                _disposables.Add(viewModel, viewModel.Subscribe(this));

                await Add(viewModel).ConfigureAwait(false);
                await Task.Delay(450).ConfigureAwait(false);
            }
        }

        private async Task InternalAddChildAsync(CancellationToken token)
        {
            using (BusyStack.GetToken())
            {
                var viewModel = new ObservableBusyViewModel();
                _disposables.Add(viewModel, viewModel.Subscribe(this));

                await Add(viewModel).ConfigureAwait(false);
                await Task.Delay(450).ConfigureAwait(false);
            }
        }

        public override void Remove(ObservableObject item)
        {
            _disposables[item].Dispose();
            base.Remove(item);
        }

        private bool CanAddChild()
        {
            return !IsBusy;
        }

        public void OnNext(bool value)
        {
            if (value)
                BusyStack.GetToken();
            else
                BusyStack.Pull();
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
    }
}
