using NUnit.Framework;

namespace MvvmScarletToolkit.Wpf.Tests
{
    public sealed class IsNotTests
    {
        [Test]
        public void Convert_Should_Return_False_For_Unsupported_DataType()
        {
            var converter = new IsNot();

            Assert.AreEqual(false, converter.Convert(new object(), null, null, null));
            Assert.AreEqual(false, converter.Convert(null, null, null, null));
            Assert.AreEqual(false, converter.Convert(1, null, null, null));
        }

        [Test]
        public void Convert_Should_Return_True_For_False()
        {
            var converter = new IsNot();

            Assert.AreEqual(true, converter.Convert(false, null, null, null));
        }

        [Test]
        public void Convert_Should_Return_False_For_True()
        {
            var converter = new IsNot();

            Assert.AreEqual(true, converter.Convert(false, null, null, null));
        }
    }
}
