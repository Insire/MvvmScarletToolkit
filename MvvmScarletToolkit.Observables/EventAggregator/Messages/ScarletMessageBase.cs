using MvvmScarletToolkit.Abstractions;
using System;

namespace MvvmScarletToolkit
{
    /// <summary>
    /// Base class for messages that provides weak refrence storage of the sender
    /// </summary>
    public abstract class ScarletMessageBase : IScarletMessage
    {
        /// <summary>
        /// Store a WeakReference to the sender just in case anyone is daft enough to keep the
        /// message around and prevent the sender from being collected.
        /// </summary>
        private readonly WeakReference _sender;

        public object Sender => _sender?.Target;

        protected ScarletMessageBase(object sender)
        {
            if (sender is null)
            {
                throw new ArgumentNullException(nameof(sender));
            }

            _sender = new WeakReference(sender);
        }
    }
}
