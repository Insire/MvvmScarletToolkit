using System;

namespace MvvmScarletToolkit
{
    internal class StrongScarletMessageSubscription<TMessage> : IScarletMessageSubscription
        where TMessage : class, IScarletMessage
    {
        protected Action<TMessage> DeliveryAction { get; set; }
        protected Func<TMessage, bool> MessageFilter { get; set; }

        public SubscriptionToken SubscriptionToken { get; }

        public bool ShouldAttemptDelivery(IScarletMessage message)
        {
            if (message == null)
                return false;

            if (!(typeof(TMessage).IsAssignableFrom(message.GetType())))
                return false;

            return MessageFilter.Invoke(message as TMessage);
        }

        public void Deliver(IScarletMessage message)
        {
            if (!(message is TMessage))
                throw new ArgumentException("Message is not the correct type");

            DeliveryAction.Invoke(message as TMessage);
        }

        public StrongScarletMessageSubscription(SubscriptionToken subscriptionToken, Action<TMessage> deliveryAction, Func<TMessage, bool> messageFilter)
        {
            SubscriptionToken = subscriptionToken ?? throw new ArgumentNullException(nameof(subscriptionToken));
            DeliveryAction = deliveryAction ?? throw new ArgumentNullException(nameof(deliveryAction));
            MessageFilter = messageFilter ?? throw new ArgumentNullException(nameof(messageFilter));
        }
    }
}
