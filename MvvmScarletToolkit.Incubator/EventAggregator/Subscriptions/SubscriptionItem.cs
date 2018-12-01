namespace MvvmScarletToolkit
{
    internal class SubscriptionItem
    {
        public IScarletMessageProxy Proxy { get; private set; }
        public IScarletMessageSubscription Subscription { get; private set; }

        public SubscriptionItem(IScarletMessageProxy proxy, IScarletMessageSubscription subscription)
        {
            Proxy = proxy;
            Subscription = subscription;
        }
    }
}
