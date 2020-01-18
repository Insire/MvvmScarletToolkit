using MvvmScarletToolkit.Abstractions;
using NUnit.Framework;
using System;

namespace MvvmScarletToolkit.Tests
{
    public sealed class SubscriptionTokenTests
    {
        [Test]
        public void Dispose_WithInvalidHubReference_DoesNotThrow()
        {
            using (var token = Utils.GetTokenWithOutOfScopeMessenger())
            {
                GC.Collect();
                GC.WaitForFullGCComplete(2000);
            }
        }

        [Test]
        public void Ctor_NullHub_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new SubscriptionToken(null));
        }

        [Test]
        public void Dispose_WithValidHubReference_UnregistersWithHub()
        {
            var messengerMock = new Moq.Mock<IScarletMessenger>();
            messengerMock.Setup((messenger) => messenger.Unsubscribe(Moq.It.IsAny<SubscriptionToken>())).Verifiable();
            var token = new SubscriptionToken(messengerMock.Object);

            token.Dispose();

            messengerMock.VerifyAll();
        }
    }
}
