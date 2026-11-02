namespace MvvmScarletToolkit.Wpf.Tests.Util
{
    /// <summary>
    /// Runs posted work immediately and in order, like a dispatcher would. The default <see cref="SynchronizationContext"/>
    /// posts to the thread pool, where a change set can overtake the one before it.
    /// </summary>
    internal sealed class InlineSynchronizationContext : SynchronizationContext
    {
        public override void Post(SendOrPostCallback d, object? state)
        {
            d(state);
        }

        public override void Send(SendOrPostCallback d, object? state)
        {
            d(state);
        }

        public override SynchronizationContext CreateCopy()
        {
            return this;
        }
    }
}
