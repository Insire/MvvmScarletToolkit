using NUnit.Framework;

namespace MvvmScarletToolkit.Wpf.Tests
{
    public sealed class IsNotNullOrEmptyTests
    {
        [Test]
        public void Convert_Should_Return_True_For_Unsupported_DataType()
        {
            var converter = new IsNotNullOrEmpty();

            Assert.AreEqual(true, converter.Convert(new object(), null, null, null));
            Assert.AreEqual(true, converter.Convert(1, null, null, null));
            Assert.AreEqual(true, converter.Convert("not null", null, null, null));
        }

        [Test]
        public void Convert_Should_Return_False_For_NullOrEmpty()
        {
            var converter = new IsNotNullOrEmpty();

            Assert.AreEqual(false, converter.Convert(null, null, null, null));
            Assert.AreEqual(false, converter.Convert(string.Empty, null, null, null));
        }
    }
}
