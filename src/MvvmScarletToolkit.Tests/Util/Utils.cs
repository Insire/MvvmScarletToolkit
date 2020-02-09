using MvvmScarletToolkit.Abstractions;
using MvvmScarletToolkit.Observables;
using System;
using System.ComponentModel;

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

        public static IScarletDispatcher GetTestDispatcher()
        {
            return NSubstitute.Substitute.For<IScarletDispatcher>();
        }

        public static IScarletCommandManager GetTestCommandManager()
        {
            return NSubstitute.Substitute.For<IScarletCommandManager>();
        }

        public static IScarletMessenger GetTestMessenger()
        {
            return NSubstitute.Substitute.For<IScarletMessenger>();
        }

        public static IExitService GetTestExitService()
        {
            return NSubstitute.Substitute.For<IExitService>();
        }

        public static IScarletEventManager<INotifyPropertyChanged, PropertyChangedEventArgs> GetTestEventManager()
        {
            return NSubstitute.Substitute.For<IScarletEventManager<INotifyPropertyChanged, PropertyChangedEventArgs>>();
        }

        public static Func<Action<bool>, IBusyStack> GetTestBusyStackFactory()
        {
            return (lambda) => new BusyStack(lambda, GetTestDispatcher());
        }

        public static ICommandBuilder GetTestCommandBuilder()
        {
            return new ScarletCommandBuilder(GetTestDispatcher(), GetTestCommandManager(), GetTestMessenger(), GetTestExitService(), GetTestEventManager(), GetTestBusyStackFactory());
        }
    }
}
