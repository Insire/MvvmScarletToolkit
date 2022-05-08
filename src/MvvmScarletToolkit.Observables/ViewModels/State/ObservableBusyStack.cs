using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Observables
{
    /// <summary>
    /// Will notify its owner and all subscribers via a provided action on if it contains more tokens
    /// </summary>
    [DebuggerDisplay("{_id}")]
    public sealed class ObservableBusyStack : IObservableBusyStack
    {
        private readonly string _id;
        private readonly ConcurrentBag<IDisposable> _items;
        private readonly ConcurrentDictionary<IObserver<bool>, object> _observers;
        private readonly Action<bool> _onChanged;

        private bool _disposed;

        public ObservableBusyStack(in Action<bool> onChanged)
        {
            _onChanged = onChanged ?? throw new ArgumentNullException(nameof(onChanged));

            _id = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
            _items = new ConcurrentBag<IDisposable>();
            _observers = new ConcurrentDictionary<IObserver<bool>, object>();
        }

        public Task Pull()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(ObservableBusyStack));
            }

            var oldValue = _items.TryPeek(out _);
            _ = _items.TryTake(out _);
            var newValue = _items.TryPeek(out _);

            if (oldValue.Equals(newValue))
            {
                return Task.CompletedTask;
            }

#if DEBUG
            Debug.WriteLine($"ObservableBusyStack({_id}) PULL HasItems: {newValue}");
#endif

            Notify(newValue);

            return Task.CompletedTask;
        }

        public Task Push(IDisposable token)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(ObservableBusyStack));
            }

            var oldValue = _items.TryPeek(out _);
            _items.Add(token);
            var newValue = _items.TryPeek(out _);

            if (oldValue.Equals(newValue))
            {
                return Task.CompletedTask;
            }

#if DEBUG
            Debug.WriteLine($"ObservableBusyStack({_id}) PUSH HasItems: {newValue}");
#endif
            Notify(newValue);

            return Task.CompletedTask;
        }

        private void Notify(bool hasItems)
        {
            NotifyOwner(hasItems);
            NotifySubscribers(hasItems);
        }

        private void NotifySubscribers(bool newValue)
        {
            var observers = _observers.Keys.ToArray();
            for (var i = 0; i < observers.Length; i++)
            {
                var observer = observers[i];
                InvokeOnChanged(observer, newValue);
            }
        }

        private void NotifyOwner(bool newValue)
        {
            InvokeOnChanged(newValue);
        }

        public IDisposable Subscribe(IObserver<bool> observer)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(ObservableBusyStack));
            }

            return new DisposalToken<bool>(observer, _observers);
        }

        [DebuggerStepThrough]
        private void InvokeOnChanged(bool newValue)
        {
            _onChanged(newValue);
        }

        [DebuggerStepThrough]
        private void InvokeOnChanged(IObserver<bool> observer, bool newValue)
        {
            if (newValue)
            {
                observer.OnNext(newValue);
            }
            else
            {
                observer.OnCompleted();
            }
        }

        /// <summary>
        /// Returns a new <see cref="IDisposable"/> thats associated with <see cref="this"/> instance of a <see cref="IDisposable"/>
        /// </summary>
        /// <returns>a new <see cref="IDisposable"/></returns>
        /// <exception cref="ObjectDisposedException"></exception>
        [DebuggerStepThrough]
        public IDisposable GetToken()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(ObservableBusyStack));
            }

            return new BusyToken(this);
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            for (var i = 0; i < _items.Count; i++)
            {
                if (_items.TryTake(out var item))
                {
                    item.Dispose();
                }
            }

            _observers.Clear();
            _disposed = true;
        }

        public override string ToString()
        {
            return $"{_id} HasItems: {!_items.IsEmpty}";
        }
    }
}
