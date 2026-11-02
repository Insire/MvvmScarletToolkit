namespace MvvmScarletToolkit.Tests
{
    public sealed class CancellableGenericScarletMessageTests : TraceTestBase
    {
        [Fact]
        public void Should_Provide_The_Content_And_The_Cancel_Action()
        {
            var isCanceled = false;

            var message = new CancellableGenericScarletMessage<string>("content", () => isCanceled = true);
            message.Cancel();

            Assert.Multiple(() =>
            {
                Assert.Equal("content", message.Value);
                Assert.True(isCanceled);
            });
        }

        [Fact]
        public void Should_Require_A_Cancel_Action()
        {
            Assert.Throws<ArgumentNullException>(() => new CancellableGenericScarletMessage<string>("content", null!));
        }
    }
}
