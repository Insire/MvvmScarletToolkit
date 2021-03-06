namespace MvvmScarletToolkit.Wpf.Samples
{
    public interface IScarletMessage
    {
        /// <summary>
        /// The sender of the message, or null if not supported by the message implementation.
        /// </summary>
        object Sender { get; }
    }
}
