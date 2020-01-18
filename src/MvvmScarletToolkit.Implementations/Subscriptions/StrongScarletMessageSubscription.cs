using MvvmScarletToolkit.Abstractions;
using System;

namespace MvvmScarletToolkit.Implementations
{
    internal sealed class StrongScarletMessageSubscription<TMessage> : IScarletMessageSubscription
        where TMessage : class, IScarletMessage
    {
        private readonly Action<TMessage> _deliveryAction;
        private readonly Func<TMessage, bool> _messageFilter;

        public SubscriptionToken Token { get; }

        public bool ShouldAttemptDelivery(IScarletMessage message)
        {
            if (message is null)
            {
                return false;
            }

            if (!(message is TMessage scarletMessage))
            {
                return false;
            }

            return _messageFilter.Invoke(scarletMessage);
        }

        public void Deliver(IScarletMessage scarletMessage)
        {
            if (!(scarletMessage is TMessage message))
            {
                throw new ArgumentException("Message is not the correct type");
            }

            _deliveryAction.Invoke(message);
        }

        public StrongScarletMessageSubscription(SubscriptionToken subscriptionToken, Action<TMessage> deliveryAction, Func<TMessage, bool> messageFilter)
        {
            _deliveryAction = deliveryAction ?? throw new ArgumentNullException(nameof(deliveryAction));
            _messageFilter = messageFilter ?? throw new ArgumentNullException(nameof(messageFilter));

            Token = subscriptionToken ?? throw new ArgumentNullException(nameof(subscriptionToken));
        }
    }
}
