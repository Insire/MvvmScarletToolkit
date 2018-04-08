namespace MvvmScarletToolkit
{
    /// <summary>
    /// Default "pass through" proxy.
    ///
    /// Does nothing other than deliver the message.
    /// </summary>
    public sealed class DefaultMessageProxy : IScarletMessageProxy
    {
        public void Deliver(IScarletMessage message, IScarletMessageSubscription subscription)
        {
            subscription.Deliver(message);
        }
    }
}
