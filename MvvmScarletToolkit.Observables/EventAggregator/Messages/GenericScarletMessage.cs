namespace MvvmScarletToolkit
{
    /// <summary>
    /// Generic message with user specified content
    /// </summary>
    /// <typeparam name="TContent">Content type to store</typeparam>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("PropertyChangedAnalyzers.PropertyChanged", "INPC001:The class has mutable properties and should implement INotifyPropertyChanged.", Justification = "Class is not meant to be bound.")]
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
