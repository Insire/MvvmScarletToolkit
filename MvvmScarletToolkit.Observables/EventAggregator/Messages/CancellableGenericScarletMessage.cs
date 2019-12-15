using System;

namespace MvvmScarletToolkit
{
    /// <summary>
    /// Basic "cancellable" generic message
    /// </summary>
    /// <typeparam name="TContent">Content type to store</typeparam>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("PropertyChangedAnalyzers.PropertyChanged", "INPC001:The class has mutable properties and should implement INotifyPropertyChanged.", Justification = "Class is not meant to be bound.")]
    public class CancellableGenericScarletMessage<TContent> : ScarletMessageBase
    {
        /// <summary>
        /// Cancel action
        /// </summary>
        public Action Cancel { get; protected set; }

        /// <summary>
        /// Contents of the message
        /// </summary>
        public TContent Content { get; protected set; }

        /// <summary>
        /// Create a new instance of the CancellableGenericTinyMessage class.
        /// </summary>
        /// <param name="sender">      Message sender (usually "this")</param>
        /// <param name="content">     Contents of the message</param>
        /// <param name="cancelAction">Action to call for cancellation</param>
        public CancellableGenericScarletMessage(object sender, TContent content, Action cancelAction)
            : base(sender)
        {
            Content = content;
            Cancel = cancelAction ?? throw new ArgumentNullException(nameof(cancelAction));
        }
    }
}
