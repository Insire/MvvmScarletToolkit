using System;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace MvvmScarletToolkit.Observables
{
    /// <summary>
    /// BusyStack will handle notifying a viewmodel on if actions are pending
    /// </summary>
    public sealed class BusyStack : IBusyStack
    {
        private readonly ConcurrentBag<BusyToken> _items;
        private readonly Action<bool> _onChanged;

        public BusyStack(Action<bool> onChanged)
        {
            _onChanged = onChanged ?? throw new ArgumentNullException(nameof(onChanged));
            _items = new ConcurrentBag<BusyToken>();
        }

        [DebuggerStepThrough]
        public void Pull()
        {
            var result = _items.TryTake(out var token);

            InvokeOnChanged(result); // could be improved to only be called, when the internal state changes
        }

        [DebuggerStepThrough]
        public void Push(BusyToken token)
        {
            _items.Add(token);

            InvokeOnChanged(true); // could be improved to only be called, when the internal state changes
        }

        [DebuggerStepThrough]
        private bool HasItems()
        {
            return _items.TryPeek(out var token);
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

        private void InvokeOnChanged(bool hasItems)
        {
            _onChanged(hasItems);
        }
    }
}
