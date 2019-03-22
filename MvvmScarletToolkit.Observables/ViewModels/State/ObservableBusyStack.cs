using MvvmScarletToolkit.Abstractions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Observables
{
    public sealed class ObservableBusyStack : IObservable<bool>, IBusyStack, IDisposable
    {
        private readonly ConcurrentBag<BusyToken> _items;
        private readonly List<IObserver<bool>> _observers;
        private readonly ReaderWriterLockSlim _syncRoot;
        private readonly Action<bool> _onChanged;
        private readonly IScarletDispatcher _dispatcher;

        public ObservableBusyStack(Action<bool> onChanged, IScarletDispatcher dispatcher)
        {
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));

            _onChanged = onChanged ?? throw new ArgumentNullException(nameof(onChanged));
            _items = new ConcurrentBag<BusyToken>();
            _observers = new List<IObserver<bool>>();
            _syncRoot = new ReaderWriterLockSlim();
        }

        [DebuggerStepThrough]
        public async Task Pull()
        {
            var oldValue = _items.TryPeek(out _);
            _ = _items.TryTake(out _);
            var newValue = _items.TryPeek(out _);

            if (oldValue.Equals(newValue))
            {
                return;
            }

            await NotifyOwner(newValue).ConfigureAwait(false);
            NotifySubscribers(newValue);
        }

        [DebuggerStepThrough]
        public async Task Push(BusyToken token)
        {
            var oldValue = _items.TryPeek(out _);
            _items.Add(token);
            var newValue = _items.TryPeek(out _);

            if (oldValue.Equals(newValue))
            {
                return;
            }

            await NotifyOwner(newValue).ConfigureAwait(false);
            NotifySubscribers(newValue);
        }

        private void NotifySubscribers(bool newValue)
        {
            _syncRoot.EnterReadLock();
            for (var i = _observers.Count - 1; i >= 0; i--)
            {
                var observer = _observers[i];
                observer.OnNext(newValue);
            }
            _syncRoot.ExitReadLock();
        }

        [DebuggerStepThrough]
        private Task NotifyOwner(bool changedValue)
        {
            return _dispatcher.Invoke(() => _onChanged.Invoke(changedValue));
        }

        public IDisposable Subscribe(IObserver<bool> observer)
        {
            _syncRoot.EnterWriteLock();
            var result = new DisposalToken(observer, _observers);
            _syncRoot.ExitWriteLock();

            return result;
        }

        /// <summary>
        /// Returns a new <see cref="BusyToken"/> thats associated with <see cref="this"/> instance of a <see cref="BusyStack"/>
        /// </summary>
        /// <returns>a new <see cref="BusyToken"/></returns>
        [DebuggerStepThrough]
        public BusyToken GetToken()
        {
            return new BusyToken(this);
        }

        public void Dispose()
        {
            _syncRoot.Dispose();
        }

        private sealed class DisposalToken : IDisposable
        {
            private readonly ICollection<IObserver<bool>> _observerCollection;
            private readonly IObserver<bool> _observer;

            public DisposalToken(IObserver<bool> observer, ICollection<IObserver<bool>> observerCollection)
            {
                _observerCollection = observerCollection ?? throw new ArgumentNullException(nameof(observerCollection));
                _observer = observer ?? throw new ArgumentNullException(nameof(observer));

                _observerCollection.Add(observer);
            }

            public void Dispose()
            {
                _observerCollection.Remove(_observer);
            }
        }
    }
}
