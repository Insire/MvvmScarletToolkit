using NUnit.Framework;

namespace MvvmScarletToolkit.Wpf.Tests
{
    public sealed class IsNotNullOrWhiteSpaceTests
    {
        [Test]
        public void Convert_Should_Return_False_For_Unsupported_DataType()
        {
            var converter = new IsNotNullOrWhiteSpace();

            Assert.AreEqual(false, converter.Convert(1, null, null, null));
            Assert.AreEqual(false, converter.Convert(new object(), null, null, null));
        }

        [Test]
        public void Convert_Should_Return_True_For_NullOrWhiteSpacey()
        {
            var converter = new IsNotNullOrWhiteSpace();

            Assert.AreEqual(false, converter.Convert(null, null, null, null));
            Assert.AreEqual(false, converter.Convert(string.Empty, null, null, null));
            Assert.AreEqual(false, converter.Convert(" ", null, null, null));
        }

        [Test]
        public void Convert_Should_Return_True_For_NonNullOrWhiteSpace()
        {
            var converter = new IsNotNullOrWhiteSpace();

            Assert.AreEqual(true, converter.Convert("not null", null, null, null));
        }
    }
}
