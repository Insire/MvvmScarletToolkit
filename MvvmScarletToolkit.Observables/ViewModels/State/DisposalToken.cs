using System;
using System.Collections.Concurrent;

namespace MvvmScarletToolkit.Observables
{
    public sealed class DisposalToken<T> : IDisposable
    {
        private readonly ConcurrentDictionary<IObserver<T>, IObserver<T>> _observerCollection;
        private readonly IObserver<T> _observer;

        public DisposalToken(IObserver<T> observer, ConcurrentDictionary<IObserver<T>, IObserver<T>> observerCollection)
        {
            _observerCollection = observerCollection ?? throw new ArgumentNullException(nameof(observerCollection));
            _observer = observer ?? throw new ArgumentNullException(nameof(observer));

            _observerCollection.AddOrUpdate(observer, p => observer, (o, p) => observer);
        }

        public void Dispose()
        {
            _observerCollection.TryRemove(_observer, out _);
        }
    }
}
