using System;

namespace MvvmScarletToolkit
{
    public interface ILogger
    {
        void Log(string message);

        void Log(Exception exception);
    }
}
