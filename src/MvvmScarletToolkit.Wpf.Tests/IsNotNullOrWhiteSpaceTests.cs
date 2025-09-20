using Xunit;

namespace MvvmScarletToolkit.Wpf.Tests
{
    public sealed class IsNotNullOrWhiteSpaceTests
    {
        [Fact]
        public void Convert_Should_Return_False_For_Unsupported_DataType()
        {
            var converter = new IsNotNullOrWhiteSpace();

            Assert.Multiple(() =>
            {
                Assert.Equal(false, converter.Convert(1, null, null, null));
                Assert.Equal(false, converter.Convert(new object (), null, null, null));
            });
        }

        [Fact]
        public void Convert_Should_Return_True_For_NullOrWhiteSpacey()
        {
            var converter = new IsNotNullOrWhiteSpace();

            Assert.Multiple(() =>
            {
                Assert.Equal(false, converter.Convert(null, null, null, null));
                Assert.Equal(false, converter.Convert(string.Empty, null, null, null));
                Assert.Equal(false, converter.Convert(" ", null, null, null));
            });
        }

        [Fact]
        public void Convert_Should_Return_True_For_NonNullOrWhiteSpace()
        {
            var converter = new IsNotNullOrWhiteSpace();

            Assert.Equal(true, converter.Convert("not null", null, null, null));
        }
    }
}
