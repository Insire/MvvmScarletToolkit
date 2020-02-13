using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;

namespace MvvmScarletToolkit
{
    /// <summary>
    /// will limit invocations of a callback queued on the dispatcher to a predefined interval
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class DispatcherProgress<T> : IProgress<T>, IDisposable
    {
        private readonly IDisposable _disposable;
        private readonly IScarletDispatcher _dispatcher;
        private readonly IObservable<EventPattern<T>> _observable;
        private readonly Action<T> _callback;

        private event EventHandler<T> ProgressChanged;

        private bool _disposed;

        public DispatcherProgress(IScarletDispatcher dispatcher, Action<T> callback, TimeSpan interval)
        {
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
            _callback = callback ?? throw new ArgumentNullException(nameof(callback));

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
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(DispatcherProgress<T>));
            }

            // queue the new value on the observable
            ProgressChanged.Invoke(this, value);
        }

        private void ReportInternal(T value)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(DispatcherProgress<T>));
            }

            _dispatcher.Invoke(() => _callback.Invoke(value), CancellationToken.None);
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposable.Dispose();
            _disposed = true;
        }
    }
}
