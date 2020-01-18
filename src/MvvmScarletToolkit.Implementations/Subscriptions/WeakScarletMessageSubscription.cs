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

        public bool ShouldAttemptDelivery(IScarletMessage scarletMessage)
        {
            if (scarletMessage is null)
            {
                return false;
            }

            if (!(scarletMessage is TMessage message))
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

            if (!(_messageFilter.Target is Func<TMessage, bool> filter))
            {
                return false;
            }

            return filter.Invoke(message);
        }

        public void Deliver(IScarletMessage scarletMessage)
        {
            if (!(scarletMessage is TMessage message))
            {
                throw new ArgumentException("Message is not the correct type");
            }

            if (!_deliveryAction.IsAlive)
            {
                return;
            }

            if (!(_deliveryAction.Target is Action<TMessage> filter))
            {
                return;
            }

            filter.Invoke(message);
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
