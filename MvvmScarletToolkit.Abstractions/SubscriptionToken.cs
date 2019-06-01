using System;

namespace MvvmScarletToolkit.Abstractions
{
    /// <summary>
    /// Represents an active subscription to a message
    /// </summary>
    public sealed class SubscriptionToken : IDisposable
    {
        private readonly WeakReference _hub;
        private readonly Type _messageType;

        public SubscriptionToken(IScarletMessenger hub, Type messageType)
        {
            if (hub is null)
            {
                throw new ArgumentNullException(nameof(hub));
            }

            if (!typeof(IScarletMessage).IsAssignableFrom(messageType))
            {
                throw new ArgumentOutOfRangeException(nameof(messageType));
            }

            _hub = new WeakReference(hub);
            _messageType = messageType;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                // free managed resources
                if (_hub.IsAlive && _hub.Target is IScarletMessenger hub)
                {
                    var unsubscribeMethod = typeof(IScarletMessenger).GetMethod("Unsubscribe", new Type[] { typeof(SubscriptionToken) });
                    unsubscribeMethod = unsubscribeMethod.MakeGenericMethod(_messageType);
                    unsubscribeMethod.Invoke(hub, new object[] { this });
                }
            }
            // free native resources if there are any.
        }
    }
}
