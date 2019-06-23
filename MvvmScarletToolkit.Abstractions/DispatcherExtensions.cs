using System;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Abstractions
{
    public static class DispatcherExtensions
    {
        public static Task Invoke(this IScarletDispatcher dispatcher, Action action)
        {
            return dispatcher.Invoke(action, CancellationToken.None);
        }

        public static Task<T> Invoke<T>(this IScarletDispatcher dispatcher, Func<T> action)
        {
            return dispatcher.Invoke(action, CancellationToken.None);
        }
    }
}
