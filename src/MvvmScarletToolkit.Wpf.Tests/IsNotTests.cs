using NUnit.Framework;

namespace MvvmScarletToolkit.Wpf.Tests
{
    public sealed class IsNotTests
    {
        [Test]
        public void Convert_Should_Return_False_For_Unsupported_DataType()
        {
            var converter = new IsNot();

            Assert.Multiple(() =>
            {
                Assert.That(converter.Convert(new object(), null, null, null), Is.EqualTo(false));
                Assert.That(converter.Convert(null, null, null, null), Is.EqualTo(false));
                Assert.That(converter.Convert(1, null, null, null), Is.EqualTo(false));
            });
        }

        [Test]
        public void Convert_Should_Return_True_For_False()
        {
            var converter = new IsNot();

            Assert.That(converter.Convert(false, null, null, null), Is.EqualTo(true));
        }

        [Test]
        public void Convert_Should_Return_False_For_True()
        {
            var converter = new IsNot();

            Assert.That(converter.Convert(false, null, null, null), Is.EqualTo(true));
        }
    }
}
