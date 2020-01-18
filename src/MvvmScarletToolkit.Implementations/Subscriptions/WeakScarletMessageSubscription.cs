using MvvmScarletToolkit.Abstractions;
using System;

namespace MvvmScarletToolkit.Implementations
{
    internal class WeakScarletMessageSubscription<TMessage> : IScarletMessageSubscription
        where TMessage : class, IScarletMessage
    {
        protected WeakReference DeliveryAction { get; set; }
        protected WeakReference MessageFilter { get; set; }

        public SubscriptionToken Token { get; }

        public bool ShouldAttemptDelivery(IScarletMessage message)
        {
            if (message is null)
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
            if (deliveryAction is null)
            {
                throw new ArgumentNullException(nameof(deliveryAction));
            }

            if (messageFilter is null)
            {
                throw new ArgumentNullException(nameof(messageFilter));
            }

            Token = subscriptionToken ?? throw new ArgumentNullException(nameof(subscriptionToken));
            DeliveryAction = new WeakReference(deliveryAction);
            MessageFilter = new WeakReference(messageFilter);
        }
    }
}
