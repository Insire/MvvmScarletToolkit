using System;
using System.Collections.Concurrent;

namespace MvvmScarletToolkit.Observables
{
    public sealed class DisposalToken<T> : IDisposable
    {
        private readonly ConcurrentDictionary<IObserver<T>, object> _observerCollection;
        private readonly IObserver<T> _observer;

        private bool _disposed;

        public DisposalToken(in IObserver<T> observer, in ConcurrentDictionary<IObserver<T>, object> observerCollection)
        {
            _observerCollection = observerCollection ?? throw new ArgumentNullException(nameof(observerCollection));
            _observer = observer ?? throw new ArgumentNullException(nameof(observer));

#pragma warning disable CS8603 // Possible null reference return.
            _ = _observerCollection.AddOrUpdate(observer, addValueFactory: _ => default, updateValueFactory: (_, __) => default);
#pragma warning restore CS8603 // Possible null reference return.
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _observerCollection.TryRemove(_observer, out _);
            _disposed = true;
        }
    }
}
