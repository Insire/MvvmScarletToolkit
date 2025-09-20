namespace MvvmScarletToolkit.Wpf.Tests
{
    [TraceTest]
    public sealed class IsNullOrEmptyTests
    {
        [Fact]
        public void Convert_Should_Return_False_For_Unsupported_DataType()
        {
            var converter = new IsNullOrEmpty();

            Assert.Multiple(() =>
            {
                Assert.Equal(false, converter.Convert(new object(), null, null, null));
                Assert.Equal(false, converter.Convert(1, null, null, null));
                Assert.Equal(false, converter.Convert("not null", null, null, null));
            });
        }

        [Fact]
        public void Convert_Should_Return_True_For_NullOrEmpty()
        {
            var converter = new IsNullOrEmpty();

            Assert.Multiple(() =>
            {
                Assert.Equal(true, converter.Convert(null, null, null, null));
                Assert.Equal(true, converter.Convert(string.Empty, null, null, null));
            });
        }
    }
}
