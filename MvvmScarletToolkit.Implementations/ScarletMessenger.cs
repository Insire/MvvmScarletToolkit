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

        private readonly object _subscriptionsPadlock;

        private readonly List<SubscriptionItem> _subscriptions;
        private readonly IScarletMessageProxy _messageProxy;

        private bool _disposed;

        private ScarletMessenger()
        {
            _subscriptions = new List<SubscriptionItem>();
            _subscriptionsPadlock = new object();
        }

        public ScarletMessenger(IScarletMessageProxy messageProxy)
            : this()
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

            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(ScarletMessenger));
            }

            lock (_subscriptionsPadlock)
            {
#pragma warning disable IDE0068 // Use recommended dispose pattern
                var token = new SubscriptionToken(this);
#pragma warning restore IDE0068 // Use recommended dispose pattern

                IScarletMessageSubscription subscription;
                if (strongReference)
                {
                    subscription = new StrongScarletMessageSubscription<TMessage>(token, deliveryAction, messageFilter);
                }
                else
                {
                    subscription = new WeakScarletMessageSubscription<TMessage>(token, deliveryAction, messageFilter);
                }

                _subscriptions.Add(new SubscriptionItem(proxy, subscription));

                return new SubscriptionToken(this);
            }
        }

        private void RemoveSubscriptionInternal(SubscriptionToken subscriptionToken)
        {
            if (subscriptionToken is null)
            {
                throw new ArgumentNullException(nameof(subscriptionToken));
            }

            SubscriptionItem[] currentlySubscribed;
            lock (_subscriptionsPadlock)
            {
                currentlySubscribed = GetSubscriptions(subscriptionToken, _subscriptions).ToArray();
            }

            currentlySubscribed.ForEach(sub => RemoveSubscription(sub, _subscriptions));
        }

        private void PublishInternal<TMessage>(TMessage message)
                where TMessage : class, IScarletMessage
        {
            if (message is null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(ScarletMessenger));
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

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(ScarletMessenger));
            }

            _disposed = true;
            if (disposing)
            {
                for (var i = _subscriptions.Count - 1; i >= 0; i--)
                {
                    var sub = _subscriptions[i];
                    RemoveSubscription(sub, _subscriptions);
                }
            }
        }

        private static void RemoveSubscription(SubscriptionItem sub, List<SubscriptionItem> subscriptions)
        {
            using (sub.Subscription.Token)
            {
                subscriptions.Remove(sub);
            }
        }

        private static IEnumerable<SubscriptionItem> GetSubscriptions(SubscriptionToken token, List<SubscriptionItem> subscriptions)
        {
            if (token is null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            return from sub in subscriptions
                   where ReferenceEquals(sub.Subscription.Token, token)
                   select sub;
        }
    }
}
