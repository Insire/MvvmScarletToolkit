using MvvmScarletToolkit.Abstractions;

namespace MvvmScarletToolkit.Tests
{
    //public class TestSubscriptionErrorHandler : ISubscriberErrorHandler
    //{
    //    public void Handle(IScarletMessage message, Exception exception)
    //    {
    //        throw exception;
    //    }
    //}

    public static class Utils
    {
        public static void FakeDeliveryAction<T>(T message)
            where T : IScarletMessage
        {
        }

        public static bool FakeMessageFilter<T>(T message)
            where T : IScarletMessage
        {
            return true;
        }

        public static SubscriptionToken GetTokenWithOutOfScopeMessenger()
        {
            var messenger = new ScarletMessenger(new ScarletMessageProxy());

            var token = new SubscriptionToken(messenger);

            return token;
        }
    }
}
