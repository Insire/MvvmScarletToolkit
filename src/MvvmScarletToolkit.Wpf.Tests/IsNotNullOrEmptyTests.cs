using NUnit.Framework;

namespace MvvmScarletToolkit.Wpf.Tests
{
    public sealed class IsNotNullOrEmptyTests
    {
        [Test]
        public void Convert_Should_Return_True_For_Unsupported_DataType()
        {
            var converter = new IsNotNullOrEmpty();

            Assert.Multiple(() =>
            {
                Assert.That(converter.Convert(new object(), null, null, null), Is.EqualTo(true));
                Assert.That(converter.Convert(1, null, null, null), Is.EqualTo(true));
                Assert.That(converter.Convert("not null", null, null, null), Is.EqualTo(true));
            });
        }

        [Test]
        public void Convert_Should_Return_False_For_NullOrEmpty()
        {
            var converter = new IsNotNullOrEmpty();

            Assert.Multiple(() =>
            {
                Assert.That(converter.Convert(null, null, null, null), Is.EqualTo(false));
                Assert.That(converter.Convert(string.Empty, null, null, null), Is.EqualTo(false));
            });
        }
    }
}
