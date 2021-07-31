using System;
using System.Threading;

namespace MvvmScarletToolkit.Commands
{
    internal sealed class NoCancellationCommand : ICancelCommand
    {
        private static readonly Lazy<NoCancellationCommand> _default = new Lazy<NoCancellationCommand>(() => new NoCancellationCommand());

        public static NoCancellationCommand Default => _default.Value;

        public CancellationToken Token { get; }

        public event EventHandler? CanExecuteChanged;

        private NoCancellationCommand()
        {
        }

        public bool CanExecute(object parameter)
        {
            return false;
        }

        public void Dispose()
        {
        }

        public void Execute(object parameter)
        {
        }

        public void NotifyCommandFinished()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        public void NotifyCommandStarting()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
