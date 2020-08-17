using System;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit
{
    public static class DispatcherExtensions
    {
        public static Task Invoke(this IScarletDispatcher dispatcher, in Action action)
        {
            return dispatcher.Invoke(action, CancellationToken.None);
        }

        public static Task<T> Invoke<T>(this IScarletDispatcher dispatcher, in Func<T> action)
        {
            return dispatcher.Invoke(action, CancellationToken.None);
        }
    }
}
