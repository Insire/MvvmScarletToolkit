using System;
using System.Diagnostics;
using System.Threading;

namespace MvvmScarletToolkit.Observables
{
    /// <summary>
    /// Will notify its owner via a provided action on if it contains more tokens
    /// </summary>
    public sealed class BusyStack : IBusyStack
    {
        private readonly Action<bool> _onChanged;

        private int _items;

        public BusyStack(in Action<bool> onChanged)
        {
            _onChanged = onChanged ?? throw new ArgumentNullException(nameof(onChanged));
        }

        public void Pull()
        {
            var oldValue = _items > 0;
            Interlocked.Decrement(ref _items);
            var newValue = _items > 0;

            if (oldValue.Equals(newValue))
            {
                return;
            }

            InvokeOnChanged(newValue);
        }

        public void Push()
        {
            var oldValue = _items > 0;
            Interlocked.Increment(ref _items);
            var newValue = _items > 0;

            if (oldValue.Equals(newValue))
            {
                return;
            }

            InvokeOnChanged(newValue);
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
        private void InvokeOnChanged(bool newValue)
        {
            _onChanged(newValue);
        }
    }
}
