﻿namespace MvvmScarletToolkit
{
    /// <summary>
    /// Represents a message subscription
    /// </summary>
    public interface IScarletMessageSubscription
    {
        /// <summary>
        /// Token returned to the subscribed to reference this subscription
        /// </summary>
        SubscriptionToken SubscriptionToken { get; }

        /// <summary>
        /// Whether delivery should be attempted.
        /// </summary>
        /// <param name="message">Message that may potentially be delivered.</param>
        /// <returns>True - ok to send, False - should not attempt to send</returns>
        bool ShouldAttemptDelivery(IScarletMessage message);

        /// <summary>
        /// Deliver the message
        /// </summary>
        /// <param name="message">Message to deliver</param>
        void Deliver(IScarletMessage message);
    }
}
