using MvvmScarletToolkit.Abstractions;
using System;
using System.Threading;
using System.Reactive;
using System.Reactive.Linq;

namespace MvvmScarletToolkit
{
    public sealed class DispatcherProgress<T> : IProgress<T>, IDisposable
    {
        private readonly IScarletDispatcher _dispatcher;
        private readonly IObservable<EventPattern<T>> _observable;
        private readonly Action<T> _action;
        private readonly IDisposable _disposable;

        private event EventHandler<T> ProgressChanged;

        private bool _isDiposed;

        public DispatcherProgress(IScarletDispatcher dispatcher, Action<T> action)
        {
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
            _action = action ?? throw new ArgumentNullException(nameof(action));

            _observable = Observable.FromEventPattern<T>(
                fsHandler => ProgressChanged += fsHandler,
                fsHandler => ProgressChanged -= fsHandler);

            _disposable = _observable
                .Sample(TimeSpan.FromMilliseconds(150))
                .Subscribe(e => ReportInternal(e.EventArgs));
        }

        public void Report(T value)
        {
            if (_isDiposed)
            {
                throw new ObjectDisposedException("DispatcherProgress");
            }

            ProgressChanged.Invoke(this, value);
        }

        private void ReportInternal(T value)
        {
            if (_isDiposed)
            {
                throw new ObjectDisposedException("DispatcherProgress");
            }

            _dispatcher.Invoke(() => _action.Invoke(value), CancellationToken.None);
        }

        public void Dispose()
        {
            _disposable.Dispose();
            _isDiposed = true;
        }
    }
}
