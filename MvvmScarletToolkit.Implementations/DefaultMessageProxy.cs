using MvvmScarletToolkit.Abstractions;
using System;

namespace MvvmScarletToolkit
{
    /// <summary>
    /// <para>Default "pass through" proxy.</para>
    /// <para>Does nothing other than deliver the message.</para>
    /// </summary>
    public sealed class ScarletMessageProxy : IScarletMessageProxy
    {
        private static readonly Lazy<ScarletMessageProxy> _default = new Lazy<ScarletMessageProxy>(() => new ScarletMessageProxy());
        public static IScarletMessageProxy Default { get; } = _default.Value;

        public void Deliver(IScarletMessage message, IScarletMessageSubscription subscription)
        {
            subscription.Deliver(message);
        }
    }
}
