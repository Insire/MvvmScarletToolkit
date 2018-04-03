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
    }
}
