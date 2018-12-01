using System;
using System.Runtime.Serialization;

namespace MvvmScarletToolkit
{
    /// <summary>
    /// Thrown when an exceptions occurs while subscribing to a message type
    /// </summary>
    [Serializable]
    public class ScarletMessengerSubscriptionException : Exception
    {
        private const string ERROR_TEXT = "Unable to add subscription for {0} : {1}";

        public ScarletMessengerSubscriptionException(Type messageType, string reason)
            : base(string.Format(ERROR_TEXT, messageType, reason))
        {
        }

        public ScarletMessengerSubscriptionException(Type messageType, string reason, Exception innerException)
            : base(string.Format(ERROR_TEXT, messageType, reason), innerException)
        {
        }

        public ScarletMessengerSubscriptionException()
        {
        }

        public ScarletMessengerSubscriptionException(string message) : base(message)
        {
        }

        public ScarletMessengerSubscriptionException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ScarletMessengerSubscriptionException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
        }
    }
}
