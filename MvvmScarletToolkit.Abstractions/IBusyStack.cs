using System;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Abstractions
{
    public interface IBusyStack
    {
        IDisposable GetToken();

        Task Pull();

        Task Push(IDisposable token);
    }
}
