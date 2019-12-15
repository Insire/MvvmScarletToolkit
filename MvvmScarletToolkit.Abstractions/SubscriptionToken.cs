using System;

namespace MvvmScarletToolkit.Abstractions
{
    /// <summary>
    /// Represents an active subscription to a message
    /// </summary>
    public sealed class SubscriptionToken : IDisposable
    {
        private readonly WeakReference _hub;

        public SubscriptionToken(IScarletMessenger hub)
        {
            if (hub is null)
            {
                throw new ArgumentNullException(nameof(hub));
            }

            _hub = new WeakReference(hub);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                // free managed resources
                if (_hub.IsAlive && _hub.Target is IScarletMessenger hub)
                {
                    var unsubscribeMethod = typeof(IScarletMessenger).GetMethod(nameof(IScarletMessenger.Unsubscribe), new Type[] { typeof(SubscriptionToken) });
                    unsubscribeMethod.Invoke(hub, new object[] { this });
                }
            }
            // free native resources if there are any.
        }
    }
}
