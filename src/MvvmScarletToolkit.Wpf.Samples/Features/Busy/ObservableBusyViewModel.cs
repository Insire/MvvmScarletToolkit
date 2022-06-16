using CommunityToolkit.Mvvm.ComponentModel;
using MvvmScarletToolkit.Observables;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MvvmScarletToolkit.Wpf.Samples
{
    [INotifyPropertyChanged]
    public sealed partial class ObservableBusyViewModel : IObservable<bool>, IDisposable
    {
        private readonly ObservableBusyStack _observableBusyStack;

        private bool _disposed;

        private bool _isBusy;
        public bool IsBusy
        {
            get { return _isBusy; }
            private set { SetProperty(ref _isBusy, value); }
        }

        public ICommand BeBusyCommand { get; }

        public ObservableBusyViewModel(IScarletCommandBuilder commandBuilder)
        {
            _observableBusyStack = new ObservableBusyStack(hasItems => { IsBusy = hasItems; Debug.WriteLine("ObservableBusyViewModel is busy: " + hasItems); });

            BeBusyCommand = commandBuilder.Create(BeBusyInternal, () => !IsBusy)
                .WithBusyNotification(_observableBusyStack)
                .WithSingleExecution()
                .Build();
        }

        private async Task BeBusyInternal(CancellationToken token)
        {
            if (_disposed)
            {
                return;
            }

            await Task.Delay(250, token);
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
