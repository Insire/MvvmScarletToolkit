using System;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Abstractions
{
    public interface IBusyStack
    {
        /// <summary>
        /// Generate a disposeable token
        /// </summary>
        /// <returns></returns>
        IDisposable GetToken();

        /// <summary>
        /// remove a token from the stack
        /// </summary>
        /// <returns></returns>
        Task Pull();

        /// <summary>
        /// add a token to the stack
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        Task Push(IDisposable token);
    }
}
