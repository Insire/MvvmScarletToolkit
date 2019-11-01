using MvvmScarletToolkit.Abstractions;
using MvvmScarletToolkit.Implementations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MvvmScarletToolkit
{
    public sealed class ScarletMessenger : IScarletMessenger
    {
        private static readonly Lazy<ScarletMessenger> _default = new Lazy<ScarletMessenger>(() => new ScarletMessenger(ScarletMessageProxy.Default));
        public static IScarletMessenger Default { get; } = _default.Value;

        private readonly object _subscriptionsPadlock = new object();
        private readonly List<SubscriptionItem> _subscriptions = new List<SubscriptionItem>();

        private readonly IScarletMessageProxy _messageProxy;

        public ScarletMessenger(IScarletMessageProxy messageProxy)
        {
            _messageProxy = messageProxy ?? throw new ArgumentNullException(nameof(messageProxy));
        }

        public SubscriptionToken Subscribe<TMessage>(Action<TMessage> deliveryAction)
            where TMessage : class, IScarletMessage
        {
            return AddSubscriptionInternal(deliveryAction, (_) => true, true, _messageProxy);
        }

        public SubscriptionToken Subscribe<TMessage>(Action<TMessage> deliveryAction, IScarletMessageProxy proxy)
            where TMessage : class, IScarletMessage
        {
            return AddSubscriptionInternal(deliveryAction, (_) => true, true, proxy);
        }

        public SubscriptionToken Subscribe<TMessage>(Action<TMessage> deliveryAction, bool useStrongReferences)
            where TMessage : class, IScarletMessage
        {
            return AddSubscriptionInternal(deliveryAction, (_) => true, useStrongReferences, _messageProxy);
        }

        public SubscriptionToken Subscribe<TMessage>(Action<TMessage> deliveryAction, bool useStrongReferences, IScarletMessageProxy proxy)
            where TMessage : class, IScarletMessage
        {
            return AddSubscriptionInternal(deliveryAction, (_) => true, useStrongReferences, proxy);
        }

        public SubscriptionToken Subscribe<TMessage>(Action<TMessage> deliveryAction, Func<TMessage, bool> messageFilter)
            where TMessage : class, IScarletMessage
        {
            return AddSubscriptionInternal(deliveryAction, messageFilter, true, _messageProxy);
        }

        public SubscriptionToken Subscribe<TMessage>(Action<TMessage> deliveryAction, Func<TMessage, bool> messageFilter, IScarletMessageProxy proxy)
            where TMessage : class, IScarletMessage
        {
            return AddSubscriptionInternal(deliveryAction, messageFilter, true, proxy);
        }

        public SubscriptionToken Subscribe<TMessage>(Action<TMessage> deliveryAction, Func<TMessage, bool> messageFilter, bool useStrongReferences)
            where TMessage : class, IScarletMessage
        {
            return AddSubscriptionInternal(deliveryAction, messageFilter, useStrongReferences, _messageProxy);
        }

        public SubscriptionToken Subscribe<TMessage>(Action<TMessage> deliveryAction, Func<TMessage, bool> messageFilter, bool useStrongReferences, IScarletMessageProxy proxy)
            where TMessage : class, IScarletMessage
        {
            return AddSubscriptionInternal(deliveryAction, messageFilter, useStrongReferences, proxy);
        }

        public void Unsubscribe(SubscriptionToken subscriptionToken)
        {
            RemoveSubscriptionInternal(subscriptionToken);
        }

        public void Publish<TMessage>(TMessage message)
            where TMessage : class, IScarletMessage
        {
            PublishInternal(message);
        }

        /// <summary>
        /// Publish a message to any subscribers asynchronously
        /// </summary>
        /// <typeparam name="TMessage">Type of message</typeparam>
        /// <param name="message">Message to deliver</param>
        public Task PublishAsync<TMessage>(TMessage message)
            where TMessage : class, IScarletMessage
        {
            return Task.Run(() => PublishInternal(message));
        }

        private SubscriptionToken AddSubscriptionInternal<TMessage>(Action<TMessage> deliveryAction, Func<TMessage, bool> messageFilter, bool strongReference, IScarletMessageProxy proxy)
                where TMessage : class, IScarletMessage
        {
            if (deliveryAction is null)
            {
                throw new ArgumentNullException(nameof(deliveryAction));
            }

            if (messageFilter is null)
            {
                throw new ArgumentNullException(nameof(messageFilter));
            }

            if (proxy is null)
            {
                throw new ArgumentNullException(nameof(proxy));
            }

            lock (_subscriptionsPadlock)
            {
                var subscriptionToken = new SubscriptionToken(this, typeof(TMessage));

                IScarletMessageSubscription subscription;
                if (strongReference)
                {
                    subscription = new StrongScarletMessageSubscription<TMessage>(subscriptionToken, deliveryAction, messageFilter);
                }
                else
                {
                    subscription = new WeakScarletMessageSubscription<TMessage>(subscriptionToken, deliveryAction, messageFilter);
                }

                _subscriptions.Add(new SubscriptionItem(proxy, subscription));

                return subscriptionToken;
            }
        }

        private void RemoveSubscriptionInternal(SubscriptionToken subscriptionToken)
        {
            if (subscriptionToken is null)
            {
                throw new ArgumentNullException(nameof(subscriptionToken));
            }

            lock (_subscriptionsPadlock)
            {
                var currentlySubscribed = (from sub in _subscriptions
                                           where ReferenceEquals(sub.Subscription.SubscriptionToken, subscriptionToken)
                                           select sub).ToArray();

                currentlySubscribed.ForEach(sub => _subscriptions.Remove(sub));
            }
        }

        private void PublishInternal<TMessage>(TMessage message)
                where TMessage : class, IScarletMessage
        {
            if (message is null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            SubscriptionItem[] currentlySubscribed;
            lock (_subscriptionsPadlock)
            {
                currentlySubscribed = (from sub in _subscriptions
                                       where sub.Subscription.ShouldAttemptDelivery(message)
                                       select sub).ToArray();
            }

            currentlySubscribed.ForEach(sub => sub.Proxy.Deliver(message, sub.Subscription));
        }
    }
}
