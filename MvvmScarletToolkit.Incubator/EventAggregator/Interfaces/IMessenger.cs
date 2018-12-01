using System;

namespace MvvmScarletToolkit
{
    /// <summary>
    /// Messenger hub responsible for taking subscriptions/publications and delivering of messages.
    /// </summary>
    public interface IMessenger
    {
        /// <summary>
        /// <para>
        /// Subscribe to a message type with the given destination and delivery action. All
        /// references are held with WeakReferences
        /// </para>
        /// <para>All messages of this type will be delivered.</para>
        /// </summary>
        /// <typeparam name="TMessage">Type of message</typeparam>
        /// <param name="deliveryAction">Action to invoke when message is delivered</param>
        /// <returns>TinyMessageSubscription used to unsubscribing</returns>
        SubscriptionToken Subscribe<TMessage>(Action<TMessage> deliveryAction) where TMessage : class, IScarletMessage;

        /// <summary>
        /// <para>
        /// Subscribe to a message type with the given destination and delivery action. Messages will
        /// be delivered via the specified proxy. All references (apart from the proxy) are held with WeakReferences
        /// </para>
        /// <para>All messages of this type will be delivered.</para>
        /// </summary>
        /// <typeparam name="TMessage">Type of message</typeparam>
        /// <param name="deliveryAction">Action to invoke when message is delivered</param>
        /// <param name="proxy">         Proxy to use when delivering the messages</param>
        /// <returns>TinyMessageSubscription used to unsubscribing</returns>
        SubscriptionToken Subscribe<TMessage>(Action<TMessage> deliveryAction, IScarletMessageProxy proxy) where TMessage : class, IScarletMessage;

        /// <summary>
        /// <para>Subscribe to a message type with the given destination and delivery action.</para>
        /// <para>All messages of this type will be delivered.</para>
        /// </summary>
        /// <typeparam name="TMessage">Type of message</typeparam>
        /// <param name="deliveryAction">     Action to invoke when message is delivered</param>
        /// <param name="useStrongReferences">Use strong references to destination and deliveryAction</param>
        /// <returns>TinyMessageSubscription used to unsubscribing</returns>
        SubscriptionToken Subscribe<TMessage>(Action<TMessage> deliveryAction, bool useStrongReferences) where TMessage : class, IScarletMessage;

        /// <summary>
        /// <para>
        /// Subscribe to a message type with the given destination and delivery action. Messages will
        /// be delivered via the specified proxy.
        /// </para>
        /// <para>All messages of this type will be delivered.</para>
        /// </summary>
        /// <typeparam name="TMessage">Type of message</typeparam>
        /// <param name="deliveryAction">     Action to invoke when message is delivered</param>
        /// <param name="useStrongReferences">Use strong references to destination and deliveryAction</param>
        /// <param name="proxy">              Proxy to use when delivering the messages</param>
        /// <returns>TinyMessageSubscription used to unsubscribing</returns>
        SubscriptionToken Subscribe<TMessage>(Action<TMessage> deliveryAction, bool useStrongReferences, IScarletMessageProxy proxy) where TMessage : class, IScarletMessage;

        /// <summary>
        /// <para>
        /// Subscribe to a message type with the given destination and delivery action with the given
        /// filter. All references are held with WeakReferences
        /// </para>
        /// <para>Only messages that "pass" the filter will be delivered.</para>
        /// </summary>
        /// <typeparam name="TMessage">Type of message</typeparam>
        /// <param name="deliveryAction">Action to invoke when message is delivered</param>
        /// <returns>TinyMessageSubscription used to unsubscribing</returns>
        SubscriptionToken Subscribe<TMessage>(Action<TMessage> deliveryAction, Func<TMessage, bool> messageFilter) where TMessage : class, IScarletMessage;

        /// <summary>
        /// <para>
        /// Subscribe to a message type with the given destination and delivery action with the given
        /// filter. Messages will be delivered via the specified proxy. All references (apart from
        /// the proxy) are held with WeakReferences
        /// </para>
        /// <para>Only messages that "pass" the filter will be delivered.</para>
        /// </summary>
        /// <typeparam name="TMessage">Type of message</typeparam>
        /// <param name="deliveryAction">Action to invoke when message is delivered</param>
        /// <param name="proxy">         Proxy to use when delivering the messages</param>
        /// <returns>TinyMessageSubscription used to unsubscribing</returns>
        SubscriptionToken Subscribe<TMessage>(Action<TMessage> deliveryAction, Func<TMessage, bool> messageFilter, IScarletMessageProxy proxy) where TMessage : class, IScarletMessage;

        /// <summary>
        /// <para>
        /// Subscribe to a message type with the given destination and delivery action with the given
        /// filter. All references are held with WeakReferences
        /// </para>
        /// <para>Only messages that "pass" the filter will be delivered.</para>
        /// </summary>
        /// <typeparam name="TMessage">Type of message</typeparam>
        /// <param name="deliveryAction">     Action to invoke when message is delivered</param>
        /// <param name="useStrongReferences">Use strong references to destination and deliveryAction</param>
        /// <returns>TinyMessageSubscription used to unsubscribing</returns>
        SubscriptionToken Subscribe<TMessage>(Action<TMessage> deliveryAction, Func<TMessage, bool> messageFilter, bool useStrongReferences) where TMessage : class, IScarletMessage;

        /// <summary>
        /// <para>
        /// Subscribe to a message type with the given destination and delivery action with the given
        /// filter. Messages will be delivered via the specified proxy. All references are held with WeakReferences
        /// </para>
        /// <para>Only messages that "pass" the filter will be delivered.</para>
        /// </summary>
        /// <typeparam name="TMessage">Type of message</typeparam>
        /// <param name="deliveryAction">     Action to invoke when message is delivered</param>
        /// <param name="useStrongReferences">Use strong references to destination and deliveryAction</param>
        /// <param name="proxy">              Proxy to use when delivering the messages</param>
        /// <returns>TinyMessageSubscription used to unsubscribing</returns>
        SubscriptionToken Subscribe<TMessage>(Action<TMessage> deliveryAction, Func<TMessage, bool> messageFilter, bool useStrongReferences, IScarletMessageProxy proxy) where TMessage : class, IScarletMessage;

        /// <summary>
        /// <para>Unsubscribe from a particular message type.</para>
        /// <para>Does not throw an exception if the subscription is not found.</para>
        /// </summary>
        /// <typeparam name="TMessage">Type of message</typeparam>
        /// <param name="subscriptionToken">Subscription token received from Subscribe</param>
        void Unsubscribe(SubscriptionToken subscriptionToken);

        /// <summary>
        /// Publish a message to any subscribers
        /// </summary>
        /// <typeparam name="TMessage">Type of message</typeparam>
        /// <param name="message">Message to deliver</param>
        void Publish<TMessage>(TMessage message) where TMessage : class, IScarletMessage;

#pragma warning disable RCS1047 // Non-asynchronous method name should not end with 'Async'.

        /// <summary>
        /// Publish a message to any subscribers asynchronously
        /// </summary>
        /// <typeparam name="TMessage">Type of message</typeparam>
        /// <param name="message">Message to deliver</param>
        void PublishAsync<TMessage>(TMessage message) where TMessage : class, IScarletMessage;

        /// <summary>
        /// Publish a message to any subscribers asynchronously
        /// </summary>
        /// <typeparam name="TMessage">Type of message</typeparam>
        /// <param name="message"> Message to deliver</param>
        /// <param name="callback">AsyncCallback called on completion</param>
        void PublishAsync<TMessage>(TMessage message, AsyncCallback callback) where TMessage : class, IScarletMessage;

#pragma warning restore RCS1047 // Non-asynchronous method name should not end with 'Async'.
    }
}
