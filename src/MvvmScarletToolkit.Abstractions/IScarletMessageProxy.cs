namespace MvvmScarletToolkit.Abstractions
{
    /// <summary>
    /// <para>Message proxy definition.</para>
    /// <para>
    /// A message proxy can be used to intercept/alter messages and/or marshall delivery actions onto
    /// a particular thread.
    /// </para>
    /// </summary>
    public interface IScarletMessageProxy
    {
        void Deliver(IScarletMessage message, IScarletMessageSubscription subscription);
    }
}
