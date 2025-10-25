namespace MvvmScarletToolkit.Wpf.Tests
{
    public sealed class IsNotTests : TraceTestBase
    {
        [Fact]
        public void Convert_Should_Return_False_For_Unsupported_DataType()
        {
            var converter = new IsNot();

            Assert.Multiple(() =>
            {
                Assert.Equal(false, converter.Convert(new object(), null, null, null));
                Assert.Equal(false, converter.Convert(null, null, null, null));
                Assert.Equal(false, converter.Convert(1, null, null, null));
            });
        }

        [Fact]
        public void Convert_Should_Return_True_For_False()
        {
            var converter = new IsNot();

            Assert.Equal(true, converter.Convert(false, null, null, null));
        }

        [Fact]
        public void Convert_Should_Return_False_For_True()
        {
            var converter = new IsNot();

            Assert.Equal(true, converter.Convert(false, null, null, null));
        }
    }
}
