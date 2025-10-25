namespace MvvmScarletToolkit.Wpf.Tests
{
    [TraceTest]
    public sealed class IsNullTests
    {
        [Fact]
        public void Convert_Should_Return_True_For_Anything_But_Null()
        {
            var converter = new IsNull();

            Assert.Multiple(() =>
            {
                Assert.Equal(false, converter.Convert(new object(), null, null, null));
                Assert.Equal(false, converter.Convert(1, null, null, null));
            });
        }

        [Fact]
        public void Convert_Should_Return_False_Null()
        {
            var converter = new IsNull();

            Assert.Equal(true, converter.Convert(null, null, null, null));
        }
    }
}
