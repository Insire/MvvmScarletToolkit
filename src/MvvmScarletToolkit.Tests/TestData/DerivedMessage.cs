namespace MvvmScarletToolkit.Tests
{
    public sealed class DerivedMessage<TThings> : TestMessage
    {
        public TThings Things { get; set; }

        public DerivedMessage(object sender)
            : base(sender)
        {
        }
    }
}
