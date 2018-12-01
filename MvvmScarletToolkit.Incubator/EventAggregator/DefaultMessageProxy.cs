namespace MvvmScarletToolkit
{
    /// <summary>
    /// <para>Default "pass through" proxy.</para>
    /// <para>Does nothing other than deliver the message.</para>
    /// </summary>
    public sealed class DefaultMessageProxy : IScarletMessageProxy
    {
        public void Deliver(IScarletMessage message, IScarletMessageSubscription subscription)
        {
            subscription.Deliver(message);
        }
    }
}
