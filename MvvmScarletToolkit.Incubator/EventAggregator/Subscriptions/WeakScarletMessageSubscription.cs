using System;

namespace MvvmScarletToolkit
{
    internal class WeakScarletMessageSubscription<TMessage> : IScarletMessageSubscription
        where TMessage : class, IScarletMessage
    {
        protected WeakReference DeliveryAction { get; set; }
        protected WeakReference MessageFilter { get; set; }

        public SubscriptionToken SubscriptionToken { get; }

        public bool ShouldAttemptDelivery(IScarletMessage message)
        {
            if (message == null)
            {
                return false;
            }

            if (!(message is TMessage))
            {
                return false;
            }

            if (!DeliveryAction.IsAlive)
            {
                return false;
            }

            if (!MessageFilter.IsAlive)
            {
                return false;
            }

            return ((Func<TMessage, bool>)MessageFilter.Target).Invoke(message as TMessage);
        }

        public void Deliver(IScarletMessage message)
        {
            if (!(message is TMessage))
            {
                throw new ArgumentException("Message is not the correct type");
            }

            if (!DeliveryAction.IsAlive)
            {
                return;
            } ((Action<TMessage>)DeliveryAction.Target).Invoke(message as TMessage);
        }

        public WeakScarletMessageSubscription(SubscriptionToken subscriptionToken, Action<TMessage> deliveryAction, Func<TMessage, bool> messageFilter)
        {
            if (deliveryAction == null)
            {
                throw new ArgumentNullException(nameof(deliveryAction));
            }

            if (messageFilter == null)
            {
                throw new ArgumentNullException(nameof(messageFilter));
            }

            SubscriptionToken = subscriptionToken ?? throw new ArgumentNullException(nameof(subscriptionToken));
            DeliveryAction = new WeakReference(deliveryAction);
            MessageFilter = new WeakReference(messageFilter);
        }
    }
}
