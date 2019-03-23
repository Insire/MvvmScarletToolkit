using MvvmScarletToolkit.Abstractions;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Observables
{
    /// <summary>
    /// BusyStack will handle notifying a viewmodel on if actions are pending
    /// </summary>
    public sealed class BusyStack : IBusyStack
    {
        private readonly ConcurrentBag<IDisposable> _items;
        private readonly Action<bool> _onChanged;
        private readonly IScarletDispatcher _dispatcher;

        public BusyStack(Action<bool> onChanged, IScarletDispatcher dispatcher)
        {
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
            _onChanged = onChanged ?? throw new ArgumentNullException(nameof(onChanged));
            _items = new ConcurrentBag<IDisposable>();
        }

        [DebuggerStepThrough]
        public Task Pull()
        {
            var result = _items.TryTake(out _);

            return InvokeOnChanged(result); // could be improved to only be called, when the internal state changes
        }

        [DebuggerStepThrough]
        public Task Push(IDisposable token)
        {
            _items.Add(token);

            return InvokeOnChanged(true); // could be improved to only be called, when the internal state changes
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

        private Task InvokeOnChanged(bool hasItems)
        {
            return _dispatcher.Invoke(() => _onChanged(hasItems));
        }
    }
}
