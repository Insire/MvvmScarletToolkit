using MvvmScarletToolkit.Abstractions;

namespace MvvmScarletToolkit.Tests
{
    public static class Utils
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Method signature required for tests")]
        public static void FakeDeliveryAction<T>(T message)
            where T : IScarletMessage
        {
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Method signature required for tests")]
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
