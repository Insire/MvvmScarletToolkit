using System;
using System.Diagnostics;

namespace MvvmScarletToolkit
{
    internal sealed class InternalLogger : ILogger
    {
        public void Log(string message)
        {
            if (Debugger.IsAttached)
                Debug.WriteLine(message);
        }

        public void Log(Exception exception)
        {
            if (Debugger.IsAttached)
                Debug.WriteLine(exception);
        }
    }
}
