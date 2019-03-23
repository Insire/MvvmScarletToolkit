using System.Threading;
using System.Windows.Input;

namespace MvvmScarletToolkit.Abstractions
{
    public interface ICancelCommand : ICommand
    {
        CancellationToken Token { get; }

        void NotifyCommandFinished();

        void NotifyCommandStarting();
    }
}
