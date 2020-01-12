using System;

namespace MvvmScarletToolkit
{
    public interface IAsyncCommand : IConcurrentCommand, IDisposable
    {
    }
}
