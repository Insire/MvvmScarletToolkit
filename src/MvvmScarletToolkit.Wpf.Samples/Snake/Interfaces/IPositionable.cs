using System;

namespace MvvmScarletToolkit.Wpf.Samples
{
    public interface IPositionable
    {
        Position CurrentPosition { get; }
        Size Size { get; }
    }

    public interface IScarletMessage
    {
        /// <summary>
        /// The sender of the message, or null if not supported by the message implementation.
        /// </summary>
        object Sender { get; }
    }

    /// <summary>
    /// Base class for messages that provides weak refrence storage of the sender
    /// </summary>
    public abstract class ScarletMessageBase : IScarletMessage
    {
        /// <summary>
        /// Store a WeakReference to the sender just in case anyone is daft enough to keep the
        /// message around and prevent the sender from being collected.
        /// </summary>
        private readonly WeakReference _sender;

        public object Sender => _sender.Target;

        protected ScarletMessageBase(in object sender)
        {
            if (sender is null)
            {
                throw new ArgumentNullException(nameof(sender));
            }

            _sender = new WeakReference(sender);
        }
    }

    /// <summary>
    /// Generic message with user specified content
    /// </summary>
    /// <typeparam name="TContent">Content type to store</typeparam>
    public class GenericScarletMessage<TContent> : ScarletMessageBase
    {
        /// <summary>
        /// Contents of the message
        /// </summary>
        public TContent Content { get; }

        public GenericScarletMessage(in object sender, in TContent content)
            : base(sender)
        {
            Content = content;
        }
    }
}
