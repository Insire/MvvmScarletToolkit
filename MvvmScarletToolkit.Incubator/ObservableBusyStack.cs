using MvvmScarletToolkit.Observables;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace MvvmScarletToolkit
{
    public sealed class ObservableBusyStack : BusyStack, IObservable<bool>, IDisposable
    {
        private readonly ConcurrentBag<BusyToken> _stack;
        private readonly List<IObserver<bool>> _observers;
        private readonly ReaderWriterLockSlim _syncRoot;

        public ObservableBusyStack(Action<bool> onChanged)
            : base(onChanged)
        {
            _stack = new ConcurrentBag<BusyToken>();
            _observers = new List<IObserver<bool>>();
            _syncRoot = new ReaderWriterLockSlim();
        }

        public override bool Pull()
        {
            var result = base.Pull() || this.Pull();
            if (result)
                Notify();

            return result;
        }

        public override void Push(BusyToken token)
        {
            base.Push(token);
            this.Push(token);

            Notify();
        }

        private void Notify()
        {
            var changedValue = _stack.TryPeek(out _);

            NotifyOwner(changedValue);
            NotifySubscribers(changedValue);
        }

        private void NotifySubscribers(bool changedValue)
        {
            _syncRoot.EnterReadLock();
            for (var i = _observers.Count; i >= 0; i--)
            {
                var observer = _observers[i];
                observer.OnNext(changedValue);
            }
            _syncRoot.ExitReadLock();
        }

        private void NotifyOwner(bool changedValue)
        {
            OnChanged.Invoke(changedValue);
        }

        public IDisposable Subscribe(IObserver<bool> observer)
        {
            _syncRoot.EnterWriteLock();
            var result = new DisposalToken(observer, _observers);
            _syncRoot.ExitWriteLock();

            return result;
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
