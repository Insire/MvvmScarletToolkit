using NUnit.Framework;

namespace MvvmScarletToolkit.Wpf.Tests
{
    public sealed class IsNullOrWhiteSpaceTests
    {
        [Test]
        public void Convert_Should_Return_False_For_Unsupported_DataType()
        {
            var converter = new IsNullOrWhiteSpace();

            Assert.AreEqual(false, converter.Convert(new object(), null, null, null));
            Assert.AreEqual(false, converter.Convert(1, null, null, null));
            Assert.AreEqual(false, converter.Convert("not null or white space", null, null, null));
        }

        [Test]
        public void Convert_Should_Return_True_For_NullOrEmpty()
        {
            var converter = new IsNullOrWhiteSpace();

            Assert.AreEqual(true, converter.Convert(null, null, null, null));
            Assert.AreEqual(true, converter.Convert(string.Empty, null, null, null));
            Assert.AreEqual(true, converter.Convert(" ", null, null, null));
        }
    }
}
