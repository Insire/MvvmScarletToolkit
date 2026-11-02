namespace MvvmScarletToolkit.Tests
{
    public sealed class EventExtensionTests : TraceTestBase
    {
        [Fact]
        public void Raise_Should_Pass_The_Sender_To_The_Handler()
        {
            object? sender = null;
            EventHandler handler = (s, _) => sender = s;

            handler.Raise("sender");

            Assert.Equal("sender", sender);
        }

        [Fact]
        public void Raise_Should_Ignore_A_Missing_Handler()
        {
            EventHandler? handler = null;

            handler.Raise("sender");
        }
    }
}
