using NUnit.Framework;
using System;

namespace MvvmScarletToolkit.Tests
{
    public sealed class MessageTests
    {
        [Test]
        public void Ctor_ShouldThrowForNullSender()
        {
            Assert.Throws<ArgumentNullException>(() => new GenericScarletMessage<object>(null, new object()));
        }

        [Test]
        public void Ctor_ShouldNotThrowForNullContent()
        {
            new GenericScarletMessage<object>(new object(), null);
        }

        [Test]
        public void Ctor_SenderShouldNotBeNull()
        {
            var sender = new object();
            var message = new GenericScarletMessage<object>(sender, null);

            Assert.IsNotNull(message.Sender);
        }
    }
}
