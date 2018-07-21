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
        private ConcurrentBag<BusyToken> _items;
        protected ConcurrentBag<BusyToken> Items
        {
            get { return _items; }
            set { SetValue(ref _items, value, InvokeOnChanged); }
        }

        private Action<bool> _onChanged;
        public Action<bool> OnChanged
        {
            get { return _onChanged; }
            set { SetValue(ref _onChanged, value); }
        }

        public BusyStack()
        {
            Items = new ConcurrentBag<BusyToken>();
        }

        public BusyStack(Action<bool> onChanged)
            : this()
        {
            OnChanged = onChanged ?? throw new ArgumentNullException(nameof(onChanged));
        }

        /// <summary>
        /// Tries to take an item from the stack and returns true if that action was successful
        /// </summary>
        /// <returns></returns>
        [DebuggerStepThrough]
        public bool Pull()
        {
            var result = Items.TryTake(out var token);

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
            Items.Add(token);

            InvokeOnChanged();
        }

        [DebuggerStepThrough]
        public bool HasItems()
        {
            return Items?.TryPeek(out var token) ?? false;
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
            if (OnChanged == null)
            {
                return;
            }

            OnChanged(HasItems());
        }
    }
}
