using System;

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
        void Pull();

        /// <summary>
        /// add a token to the stack
        /// </summary>
        /// <param name="token"></param>
        void Push();
    }
}
