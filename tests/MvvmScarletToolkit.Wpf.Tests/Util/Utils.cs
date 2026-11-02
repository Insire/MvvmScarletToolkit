using CommunityToolkit.Mvvm.Messaging;
using MvvmScarletToolkit.Observables;
using System.ComponentModel;

namespace MvvmScarletToolkit.Wpf.Tests.Util
{
    internal static class Utils
    {
        public static IScarletExceptionHandler GetTestExceptionHandler()
        {
            return Substitute.For<IScarletExceptionHandler>();
        }

        public static IScarletDispatcher GetTestDispatcher()
        {
            return Substitute.For<IScarletDispatcher>();
        }

        public static IScarletCommandManager GetTestCommandManager()
        {
            return Substitute.For<IScarletCommandManager>();
        }

        public static IMessenger GetTestMessenger()
        {
            return Substitute.For<IMessenger>();
        }

        public static IExitService GetTestExitService()
        {
            return Substitute.For<IExitService>();
        }

        public static IScarletEventManager<INotifyPropertyChanged, PropertyChangedEventArgs> GetTestEventManager()
        {
            return Substitute.For<IScarletEventManager<INotifyPropertyChanged, PropertyChangedEventArgs>>();
        }

        public static Func<Action<bool>, IBusyStack> GetTestBusyStackFactory()
        {
            return (lambda) => new BusyStack(lambda);
        }

        public static IScarletCommandBuilder GetTestCommandBuilder(IScarletDispatcher? dispatcher = null,
                                                                   IScarletCommandManager? commandManager = null,
                                                                   IScarletExceptionHandler? exceptionHandler = null,
                                                                   IMessenger? messenger = null,
                                                                   IExitService? exitService = null,
                                                                   IScarletEventManager<INotifyPropertyChanged, PropertyChangedEventArgs>? eventManager = null,
                                                                   Func<Action<bool>, IBusyStack>? busyStackFactory = null)
        {
            return new ScarletCommandBuilder(dispatcher ?? GetTestDispatcher(),
                                             commandManager ?? GetTestCommandManager(),
                                             exceptionHandler ?? GetTestExceptionHandler(),
                                             messenger ?? GetTestMessenger(),
                                             exitService ?? GetTestExitService(),
                                             eventManager ?? GetTestEventManager(),
                                             busyStackFactory ?? GetTestBusyStackFactory());
        }

        public static ICancelCommand GetTestCancelCommand()
        {
            return Substitute.For<ICancelCommand>();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Method signature required for testcases")]
        public static IBusyStack TestBusyStackFactory(Action<bool> lambda)
        {
            return Substitute.For<IBusyStack>();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Method signature required for testcases")]
        public static Task TestExecute(object arg, CancellationToken token)
        {
            return Task.CompletedTask;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Method signature required for testcases")]
        public static bool TestCanExecute(object arg)
        {
            return true;
        }
    }
}
