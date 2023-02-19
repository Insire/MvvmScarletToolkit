using NUnit.Framework;

namespace MvvmScarletToolkit.Wpf.Tests
{
    public sealed class IsNotNullTests
    {
        [Test]
        public void Convert_Should_Return_True_For_Anything_But_Null()
        {
            var converter = new IsNotNull();

            Assert.AreEqual(true, converter.Convert(new object(), null, null, null));
            Assert.AreEqual(true, converter.Convert(1, null, null, null));
        }

        [Test]
        public void Convert_Should_Return_False_Null()
        {
            var converter = new IsNotNull();

            Assert.AreEqual(false, converter.Convert(null, null, null, null));
        }
    }
}
