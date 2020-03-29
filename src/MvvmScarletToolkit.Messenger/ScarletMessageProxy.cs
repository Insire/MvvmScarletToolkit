using MvvmScarletToolkit.Abstractions;
using System;
using System.Diagnostics;

namespace MvvmScarletToolkit
{
    /// <summary>
    /// <para>Default "pass through" proxy.</para>
    /// <para>Does nothing other than deliver the message.</para>
    /// </summary>
    public sealed class ScarletMessageProxy : IScarletMessageProxy
    {
        private static readonly Lazy<ScarletMessageProxy> _default = new Lazy<ScarletMessageProxy>(() => new ScarletMessageProxy());
        public static IScarletMessageProxy Default => _default.Value;

        public void Deliver(IScarletMessage message, IScarletMessageSubscription subscription)
        {
            subscription.Deliver(message);

#if DEBUG
            Debug.WriteLine("ScarletMessageProxy: Deliver --> " + message.GetType().GetGenericTypeName() + " via " + subscription.GetType().GetGenericTypeName());
#endif
        }
    }
}
