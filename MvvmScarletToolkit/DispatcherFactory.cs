using System;
using System.Windows.Threading;

namespace MvvmScarletToolkit
{
    public static class DispatcherFactory
    {
        public static Dispatcher GetDispatcher()
        {
            return Dispatcher.CurrentDispatcher;
        }

        public static void Invoke(Action action)
        {
            var dispatcher = GetDispatcher();

            if (dispatcher.CheckAccess())
                action();
            else
                dispatcher.BeginInvoke(action);
        }

        public static void Invoke(Action<object> action, object[] parameter, DispatcherPriority priority = DispatcherPriority.Normal)
        {
            var dispatcher = GetDispatcher();

            if (dispatcher.CheckAccess())
                action(parameter);
            else
                dispatcher.BeginInvoke(action, priority, parameter);
        }
    }
}
