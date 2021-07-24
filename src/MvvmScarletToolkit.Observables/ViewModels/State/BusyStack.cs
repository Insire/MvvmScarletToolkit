using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Observables
{
    /// <summary>
    /// Will notify its owner via a provided action on if it contains more tokens
    /// </summary>
    public sealed class BusyStack : IBusyStack
    {
        private readonly ConcurrentBag<IDisposable> _items;
        private readonly Action<bool> _onChanged;
        private readonly IScarletDispatcher _dispatcher;

        public BusyStack(in Action<bool> onChanged, in IScarletDispatcher dispatcher)
        {
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
            _onChanged = onChanged ?? throw new ArgumentNullException(nameof(onChanged));
            _items = new ConcurrentBag<IDisposable>();
        }

        public async Task Pull()
        {
            var oldValue = _items.TryPeek(out _);
            _ = _items.TryTake(out _);
            var newValue = _items.TryPeek(out _);

            if (oldValue.Equals(newValue))
            {
                return;
            }

            await InvokeOnChanged(newValue).ConfigureAwait(false);
        }

        public async Task Push(IDisposable token)
        {
            var oldValue = _items.TryPeek(out _);
            _items.Add(token);
            var newValue = _items.TryPeek(out _);

            if (oldValue.Equals(newValue))
            {
                return;
            }

            await InvokeOnChanged(newValue).ConfigureAwait(false);
        }

        /// <summary>
        /// Returns a new <see cref="IDisposable"/> thats associated with <see cref="this"/> instance of a <see cref="IDisposable"/>
        /// </summary>
        /// <returns>a new <see cref="IDisposable"/></returns>
        [DebuggerStepThrough]
        public IDisposable GetToken()
        {
            return new BusyToken(this);
        }

        [DebuggerStepThrough]
        private Task InvokeOnChanged(bool newValue)
        {
            return _dispatcher.Invoke(() => _onChanged(newValue));
        }
    }
}
