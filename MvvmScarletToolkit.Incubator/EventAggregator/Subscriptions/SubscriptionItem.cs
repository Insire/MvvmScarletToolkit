using MvvmScarletToolkit.Abstractions;

namespace MvvmScarletToolkit
{
    internal class SubscriptionItem
    {
        public IScarletMessageProxy Proxy { get; }
        public IScarletMessageSubscription Subscription { get; }

        public SubscriptionItem(IScarletMessageProxy proxy, IScarletMessageSubscription subscription)
        {
            Proxy = proxy;
            Subscription = subscription;
        }
    }
}
