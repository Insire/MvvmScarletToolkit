using System;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit
{
    public interface IScarletDispatcher
    {
        Task Invoke(Action action, CancellationToken token);

        Task<T> Invoke<T>(Func<T> action, CancellationToken token);
    }
}
