using System;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace MvvmScarletToolkit.Observables
{
    /// <summary>
    /// BusyStack will handle notifying a viewmodel on if actions are pending
    /// </summary>
    public class BusyStack
    {
        private readonly ConcurrentBag<BusyToken> _items;
        protected readonly Action<bool> OnChanged;

        public BusyStack(Action<bool> onChanged)
        {
            OnChanged = onChanged ?? throw new ArgumentNullException(nameof(onChanged));
            _items = new ConcurrentBag<BusyToken>();
        }

        [DebuggerStepThrough]
        public virtual bool Pull()
        {
            var result = _items.TryTake(out var token);

            if (result)
                InvokeOnChanged();

            return result;
        }

        [DebuggerStepThrough]
        public virtual void Push(BusyToken token)
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
            OnChanged(HasItems());
        }
    }
}
