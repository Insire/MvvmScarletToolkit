using NUnit.Framework;

namespace MvvmScarletToolkit.Wpf.Tests
{
    public sealed class IsNullTests
    {
        [Test]
        public void Convert_Should_Return_True_For_Anything_But_Null()
        {
            var converter = new IsNull();

            Assert.Multiple(() =>
            {
                Assert.That(converter.Convert(new object(), null, null, null), Is.EqualTo(false));
                Assert.That(converter.Convert(1, null, null, null), Is.EqualTo(false));
            });
        }

        [Test]
        public void Convert_Should_Return_False_Null()
        {
            var converter = new IsNull();

            Assert.That(converter.Convert(null, null, null, null), Is.EqualTo(true));
        }
    }
}
