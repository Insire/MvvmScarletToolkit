using Microsoft.Toolkit.Mvvm.Messaging;
using MvvmScarletToolkit.Abstractions;
using MvvmScarletToolkit.Observables;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Tests
{
    internal static class Utils
    {
        public static IScarletDispatcher GetTestDispatcher()
        {
            return NSubstitute.Substitute.For<IScarletDispatcher>();
        }

        public static IScarletCommandManager GetTestCommandManager()
        {
            return NSubstitute.Substitute.For<IScarletCommandManager>();
        }

        public static IMessenger GetTestMessenger()
        {
            return NSubstitute.Substitute.For<IMessenger>();
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

        public static IScarletCommandBuilder GetTestCommandBuilder()
        {
            return new ScarletCommandBuilder(GetTestDispatcher(), GetTestCommandManager(), GetTestMessenger(), GetTestExitService(), GetTestEventManager(), GetTestBusyStackFactory());
        }

        public static ICancelCommand GetTestCancelCommand()
        {
            return NSubstitute.Substitute.For<ICancelCommand>();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Method signature required for testcases")]
        public static IBusyStack TestBusyStackFactory(Action<bool> lambda)
        {
            return NSubstitute.Substitute.For<IBusyStack>();
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
