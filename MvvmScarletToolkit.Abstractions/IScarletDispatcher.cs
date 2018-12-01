using System;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Abstractions
{
    public interface IScarletDispatcher
    {
        Task Invoke(Action action);

        Task<T> Invoke<T>(Func<T> action);
    }
}
