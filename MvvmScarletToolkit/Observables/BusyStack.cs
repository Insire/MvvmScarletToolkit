using System;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace MvvmScarletToolkit
{
    /// <summary>
    /// BusyStack will handle notifying a viewmodel on if actions are pending
    /// </summary>
    public class BusyStack : ObservableObject
    {
        private readonly ConcurrentBag<BusyToken> _items;
        private readonly Action<bool> _onChanged;

        public BusyStack()
        {
            _items = new ConcurrentBag<BusyToken>();
        }

        public BusyStack(Action<bool> onChanged)
            : this()
        {
            _onChanged = onChanged ?? throw new ArgumentNullException(nameof(onChanged));
        }

        /// <summary>
        /// Tries to take an item from the stack and returns true if that action was successful
        /// </summary>
        /// <returns></returns>
        [DebuggerStepThrough]
        public bool Pull()
        {
            var result = _items.TryTake(out var token);

            if (result)
            {
                InvokeOnChanged();
            }

            return result;
        }

        /// <summary>
        /// Adds a new <see cref="BusyToken"/> to the Stack
        /// </summary>
        [DebuggerStepThrough]
        public void Push(BusyToken token)
        {
            _items.Add(token);

            InvokeOnChanged();
        }

        [DebuggerStepThrough]
        public bool HasItems()
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

        private void InvokeOnChanged()
        {
            _onChanged(HasItems());
        }
    }
}
