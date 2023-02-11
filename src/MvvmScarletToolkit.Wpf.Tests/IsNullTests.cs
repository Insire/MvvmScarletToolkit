using NUnit.Framework;

namespace MvvmScarletToolkit.Wpf.Tests
{
    public sealed class IsNullTests
    {
        [Test]
        public void Convert_Should_Return_True_For_Anything_But_Null()
        {
            var converter = new IsNull();

            Assert.AreEqual(false, converter.Convert(new object(), null, null, null));
            Assert.AreEqual(false, converter.Convert(1, null, null, null));
        }

        [Test]
        public void Convert_Should_Return_False_Null()
        {
            var converter = new IsNull();

            Assert.AreEqual(true, converter.Convert(null, null, null, null));
        }
    }
}
