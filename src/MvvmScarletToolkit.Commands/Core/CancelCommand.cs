using System.Windows.Input;

namespace MvvmScarletToolkit.Commands
{
    internal sealed class CancelCommand : ICancelCommand
    {
        private readonly IScarletCommandManager _commandManager;

        private CancellationTokenSource? _cts;
        private int _runningExecutions;

        public CancellationToken Token => Volatile.Read(ref _cts)?.Token ?? CancellationToken.None;

        public CancelCommand(in IScarletCommandManager commandManager)
        {
            _commandManager = commandManager ?? throw new ArgumentNullException(nameof(commandManager));
        }

        public void NotifyCommandStarting()
        {
            // the owning command invalidates the requery state after this call, so there is no need to raise CanExecuteChanged here
            Interlocked.Increment(ref _runningExecutions);
            if (Volatile.Read(ref _cts) is null)
            {
                Interlocked.CompareExchange(ref _cts, new CancellationTokenSource(), null);
            }
        }

        public void NotifyCommandFinished()
        {
            Interlocked.Decrement(ref _runningExecutions);
        }

        bool ICommand.CanExecute(object? parameter)
        {
            return Volatile.Read(ref _runningExecutions) > 0 && Volatile.Read(ref _cts) is not null;
        }

        void ICommand.Execute(object? parameter)
        {
            // the source is not disposed, since running executions may still register callbacks on its token
            Interlocked.Exchange(ref _cts, null)?.Cancel();

            RaiseCanExecuteChanged();
        }

        public event EventHandler CanExecuteChanged
        {
            add { _commandManager.RequerySuggested += value; }
            remove { _commandManager.RequerySuggested -= value; }
        }

        private void RaiseCanExecuteChanged()
        {
            _commandManager.InvalidateRequerySuggested();
        }

        public void Dispose()
        {
            Interlocked.Exchange(ref _cts, null)?.Dispose();
        }
    }
}
