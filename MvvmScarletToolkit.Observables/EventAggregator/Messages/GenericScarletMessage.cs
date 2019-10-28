using MvvmScarletToolkit.Observables;

namespace MvvmScarletToolkit
{
    /// <summary>
    /// Generic message with user specified content
    /// </summary>
    /// <typeparam name="TContent">Content type to store</typeparam>
    public class GenericScarletMessage<TContent> : ScarletMessageBase
    {
        /// <summary>
        /// Contents of the message
        /// </summary>
        public TContent Content { get; protected set; }

        public GenericScarletMessage(object sender, TContent content)
            : base(sender)
        {
            Content = content;
        }
    }
}
