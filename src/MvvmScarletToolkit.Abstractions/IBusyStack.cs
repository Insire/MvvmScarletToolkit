using System;
using System.Threading.Tasks;

namespace MvvmScarletToolkit
{
    public interface IBusyStack
    {
        /// <summary>
        /// Generate a disposeable token
        /// </summary>
        IDisposable GetToken();

        /// <summary>
        /// remove a token from the stack
        /// </summary>
        Task Pull();

        /// <summary>
        /// add a token to the stack
        /// </summary>
        /// <param name="token"></param>
        Task Push(IDisposable token);
    }
}
