using MvvmScarletToolkit.Abstractions;
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
    public sealed partial class ObservableBusyStack : IObservableBusyStack
    {
        private readonly string _id;
        private readonly ConcurrentBag<IDisposable> _items;
        private readonly ConcurrentDictionary<IObserver<bool>, object> _observers;
        private readonly Action<bool> _onChanged;
        private readonly IScarletDispatcher _dispatcher;

        private bool _disposed;

        public ObservableBusyStack(in Action<bool> onChanged, in IScarletDispatcher dispatcher)
        {
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
            _onChanged = onChanged ?? throw new ArgumentNullException(nameof(onChanged));

            _id = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
            _items = new ConcurrentBag<IDisposable>();
            _observers = new ConcurrentDictionary<IObserver<bool>, object>();
        }

        public async Task Pull()
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
                return;
            }

#if DEBUG
            Debug.WriteLine($"ObservableBusyStack({_id}) PULL HasItems: {newValue}");
#endif

            await Notify(newValue).ConfigureAwait(false);
        }

        public async Task Push(IDisposable token)
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
                return;
            }

#if DEBUG
            Debug.WriteLine($"ObservableBusyStack({_id}) PUSH HasItems: {newValue}");
#endif
            await Notify(newValue).ConfigureAwait(false);
        }

        private async Task Notify(bool hasItems)
        {
            var owner = NotifyOwner(hasItems);
            var subscriber = NotifySubscribers(hasItems);

            await Task.WhenAll(owner, subscriber).ConfigureAwait(false);
        }

        private async Task NotifySubscribers(bool newValue)
        {
            var observers = _observers.Keys.ToArray();
            for (var i = 0; i < observers.Length; i++)
            {
                var observer = observers[i];
                await InvokeOnChanged(observer, newValue).ConfigureAwait(false);
            }
        }

        private Task NotifyOwner(bool newValue)
        {
            return InvokeOnChanged(newValue);
        }

        public IDisposable Subscribe(IObserver<bool> observer)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(ObservableBusyStack));
            }

            var result = new DisposalToken<bool>(observer, _observers);

            return result;
        }

        [DebuggerStepThrough]
        private Task InvokeOnChanged(bool newValue)
        {
            return _dispatcher.Invoke(() => _onChanged(newValue));
        }

        [DebuggerStepThrough]
        private Task InvokeOnChanged(IObserver<bool> observer, bool newValue)
        {
            if (newValue)
            {
                return _dispatcher.Invoke(() => observer.OnNext(newValue));
            }
            else
            {
                return _dispatcher.Invoke(() => observer.OnCompleted());
            }
        }

        /// <summary>
        /// Returns a new <see cref="IDisposable"/> thats associated with <see cref="this"/> instance of a <see cref="IDisposable"/>
        /// </summary>
        /// <returns>a new <see cref="IDisposable"/></returns>
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
