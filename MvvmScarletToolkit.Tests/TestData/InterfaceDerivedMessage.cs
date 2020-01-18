namespace MvvmScarletToolkit.Tests
{
    public class InterfaceDerivedMessage<TThings> : ITestMessageInterface
    {
        public object Sender { get; private set; }

        public TThings Things { get; set; }

        public InterfaceDerivedMessage(object sender)
        {
            Sender = sender;
        }
    }
}
