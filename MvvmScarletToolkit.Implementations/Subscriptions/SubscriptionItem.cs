using MvvmScarletToolkit.Abstractions;

namespace MvvmScarletToolkit.Implementations
{
    internal sealed class SubscriptionItem
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
