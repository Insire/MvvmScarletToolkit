using System;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Abstractions
{
    public interface IScarletDispatcher
    {
        Task Invoke(Action action, CancellationToken token);

        Task<T> Invoke<T>(Func<T> action, CancellationToken token);
    }

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
