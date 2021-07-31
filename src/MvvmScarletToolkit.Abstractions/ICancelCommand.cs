using System;
using System.Threading;
using System.Windows.Input;

namespace MvvmScarletToolkit
{
    public interface ICancelCommand : ICommand, IDisposable
    {
        CancellationToken Token { get; }

        void NotifyCommandFinished();

        void NotifyCommandStarting();
    }
}
