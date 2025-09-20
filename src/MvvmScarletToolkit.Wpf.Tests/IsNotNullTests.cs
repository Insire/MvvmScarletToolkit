using Xunit;

namespace MvvmScarletToolkit.Wpf.Tests
{
    public sealed class IsNotNullTests
    {
        [Fact]
        public void Convert_Should_Return_True_For_Anything_But_Null()
        {
            var converter = new IsNotNull();

            Assert.Multiple(() =>
            {
                Assert.Equal(true, converter.Convert(new object (), null, null, null));
                Assert.Equal(true, converter.Convert(1, null, null, null));
            });
        }

        [Fact]
        public void Convert_Should_Return_False_Null()
        {
            var converter = new IsNotNull();

            Assert.Equal(false, converter.Convert(null, null, null, null));
        }
    }
}
