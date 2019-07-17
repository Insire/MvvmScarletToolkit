using MvvmScarletToolkit;
using MvvmScarletToolkit.Commands;
using MvvmScarletToolkit.Observables;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace DemoApp
{
    public sealed class ObservableBusyViewModel : ObservableObject, IObservable<bool>, IDisposable
    {
        private readonly ObservableBusyStack _observableBusyStack;

        private bool _isBusy;
        private bool _disposed;

        public bool IsBusy
        {
            get { return _isBusy; }
            private set { SetValue(ref _isBusy, value); }
        }

        public ConcurrentCommandBase BeBusyCommand { get; }

        public ObservableBusyViewModel(ICommandBuilder commandBuilder, IScarletDispatcher dispatcher)
        {
            _observableBusyStack = new ObservableBusyStack(hasItems => { IsBusy = hasItems; Debug.WriteLine("ObservableBusyViewModel is busy: " + hasItems); }, dispatcher);
            BeBusyCommand = commandBuilder.Create(BeBusyInternal, () => !IsBusy);
        }

        private async Task BeBusyInternal(CancellationToken token)
        {
            if (_disposed)
            {
                return;
            }

            using (_observableBusyStack.GetToken())
            {
                await Task.Delay(250, token).ConfigureAwait(false);
            }
        }

        public IDisposable Subscribe(IObserver<bool> observer)
        {
            ThrowIfDisposed();

            return _observableBusyStack.Subscribe(observer);
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _observableBusyStack.Dispose();
            _disposed = true;
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }
    }
}
