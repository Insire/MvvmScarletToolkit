namespace MvvmScarletToolkit.Wpf.Tests
{
    public sealed class IsNotNullOrEmptyTests : TraceTestBase
    {
        [Fact]
        public void Convert_Should_Return_True_For_Unsupported_DataType()
        {
            var converter = new IsNotNullOrEmpty();

            Assert.Multiple(() =>
            {
                Assert.Equal(true, converter.Convert(new object(), null, null, null));
                Assert.Equal(true, converter.Convert(1, null, null, null));
                Assert.Equal(true, converter.Convert("not null", null, null, null));
            });
        }

        [Fact]
        public void Convert_Should_Return_False_For_NullOrEmpty()
        {
            var converter = new IsNotNullOrEmpty();

            Assert.Multiple(() =>
            {
                Assert.Equal(false, converter.Convert(null, null, null, null));
                Assert.Equal(false, converter.Convert(string.Empty, null, null, null));
            });
        }
    }
}
