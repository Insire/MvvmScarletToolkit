using MvvmScarletToolkit.Abstractions;
using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;

namespace MvvmScarletToolkit
{
    public sealed class DispatcherProgress<T> : IProgress<T>, IDisposable
    {
        private readonly IDisposable _disposable;
        private readonly IScarletDispatcher _dispatcher;
        private readonly IObservable<EventPattern<T>> _observable;
        private readonly Action<T> _action;

        private event EventHandler<T> ProgressChanged;

        private bool _isDiposed;

        public DispatcherProgress(IScarletDispatcher dispatcher, Action<T> action, TimeSpan interval)
        {
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
            _action = action ?? throw new ArgumentNullException(nameof(action));

            _observable = Observable.FromEventPattern<T>(
                fsHandler => ProgressChanged += fsHandler,
                fsHandler => ProgressChanged -= fsHandler);

            _disposable = _observable
                .ObserveOn(System.Reactive.Concurrency.TaskPoolScheduler.Default)
                .Sample(interval)
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
