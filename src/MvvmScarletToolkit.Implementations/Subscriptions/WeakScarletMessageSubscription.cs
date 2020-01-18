using MvvmScarletToolkit.Abstractions;
using System;

namespace MvvmScarletToolkit.Implementations
{
    internal sealed class WeakScarletMessageSubscription<TMessage> : IScarletMessageSubscription
        where TMessage : class, IScarletMessage
    {
        private readonly WeakReference _deliveryAction;
        private readonly WeakReference _messageFilter;

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

            if (!_deliveryAction.IsAlive)
            {
                return false;
            }

            if (!_messageFilter.IsAlive)
            {
                return false;
            }

            return ((Func<TMessage, bool>)_messageFilter.Target).Invoke(message as TMessage);
        }

        public void Deliver(IScarletMessage message)
        {
            if (!(message is TMessage))
            {
                throw new ArgumentException("Message is not the correct type");
            }

            if (!_deliveryAction.IsAlive)
            {
                return;
            }

            ((Action<TMessage>)_deliveryAction.Target).Invoke(message as TMessage);
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

            _deliveryAction = new WeakReference(deliveryAction);
            _messageFilter = new WeakReference(messageFilter);

            Token = subscriptionToken ?? throw new ArgumentNullException(nameof(subscriptionToken));
        }
    }
}