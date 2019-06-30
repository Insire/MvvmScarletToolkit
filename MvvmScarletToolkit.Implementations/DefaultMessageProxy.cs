using MvvmScarletToolkit.Abstractions;

namespace MvvmScarletToolkit
{
    /// <summary>
    /// <para>Default "pass through" proxy.</para>
    /// <para>Does nothing other than deliver the message.</para>
    /// </summary>
    public sealed class ScarletMessageProxy : IScarletMessageProxy
    {
        public static IScarletMessageProxy Default { get; } = new ScarletMessageProxy();

        public void Deliver(IScarletMessage message, IScarletMessageSubscription subscription)
        {
            subscription.Deliver(message);
        }
    }
}
