using System;
using System.Collections.Generic;
using System.Linq;

namespace MvvmScarletToolkit
{
    public sealed partial class ScarletMessenger : IMessenger
    {
        private readonly object _subscriptionsPadlock = new object();
        private readonly List<SubscriptionItem> _subscriptions = new List<SubscriptionItem>();

        private readonly IScarletMessageProxy _messageProxy;
        private readonly ILogger _log;

        public ScarletMessenger(ILogger log, IScarletMessageProxy messageProxy)
        {
            _log = log ?? throw new ArgumentNullException(nameof(log));
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

#pragma warning disable RCS1047 // Non-asynchronous method name should not end with 'Async'.

        /// <summary>
        /// Publish a message to any subscribers asynchronously
        /// </summary>
        /// <typeparam name="TMessage">Type of message</typeparam>
        /// <param name="message">Message to deliver</param>
        public void PublishAsync<TMessage>(TMessage message)

            where TMessage : class, IScarletMessage
        {
            PublishAsyncInternal(message, null);
        }

        /// <summary>
        /// Publish a message to any subscribers asynchronously
        /// </summary>
        /// <typeparam name="TMessage">Type of message</typeparam>
        /// <param name="message"> Message to deliver</param>
        /// <param name="callback">AsyncCallback called on completion</param>
        public void PublishAsync<TMessage>(TMessage message, AsyncCallback callback)
            where TMessage : class, IScarletMessage
        {
            PublishAsyncInternal(message, callback);
        }

#pragma warning restore RCS1047 // Non-asynchronous method name should not end with 'Async'.

        private SubscriptionToken AddSubscriptionInternal<TMessage>(Action<TMessage> deliveryAction, Func<TMessage, bool> messageFilter, bool strongReference, IScarletMessageProxy proxy)
                where TMessage : class, IScarletMessage
        {
            if (deliveryAction == null)
                throw new ArgumentNullException(nameof(deliveryAction));

            if (messageFilter == null)
                throw new ArgumentNullException(nameof(messageFilter));

            if (proxy == null)
                throw new ArgumentNullException(nameof(proxy));

            lock (_subscriptionsPadlock)
            {
                var subscriptionToken = new SubscriptionToken(this, typeof(TMessage));

                IScarletMessageSubscription subscription;
                if (strongReference)
                    subscription = new StrongScarletMessageSubscription<TMessage>(subscriptionToken, deliveryAction, messageFilter);
                else
                    subscription = new WeakScarletMessageSubscription<TMessage>(subscriptionToken, deliveryAction, messageFilter);

                _subscriptions.Add(new SubscriptionItem(proxy, subscription));

                return subscriptionToken;
            }
        }

        private void RemoveSubscriptionInternal(SubscriptionToken subscriptionToken)
        {
            if (subscriptionToken == null)
                throw new ArgumentNullException(nameof(subscriptionToken));

            lock (_subscriptionsPadlock)
            {
                var currentlySubscribed = (from sub in _subscriptions
                                           where ReferenceEquals(sub.Subscription.SubscriptionToken, subscriptionToken)
                                           select sub).ToList();

                currentlySubscribed.ForEach(sub => _subscriptions.Remove(sub));
            }
        }

        private void PublishInternal<TMessage>(TMessage message)
                where TMessage : class, IScarletMessage
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            List<SubscriptionItem> currentlySubscribed;
            lock (_subscriptionsPadlock)
            {
                currentlySubscribed = (from sub in _subscriptions
                                       where sub.Subscription.ShouldAttemptDelivery(message)
                                       select sub).ToList();
            }

            currentlySubscribed.ForEach(sub => sub.Proxy.Deliver(message, sub.Subscription));
        }

        private void PublishAsyncInternal<TMessage>(TMessage message, AsyncCallback callback)
            where TMessage : class, IScarletMessage
        {
            ((Action)(() => PublishInternal(message))).BeginInvoke(callback, null);
        }
    }
}
