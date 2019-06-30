using MvvmScarletToolkit.Abstractions;
using System;
using System.Threading;
using System.Windows.Input;

namespace MvvmScarletToolkit.Commands
{
    internal sealed class CancelCommand : ICancelCommand
    {
        private readonly IScarletCommandManager _commandManager;

        private CancellationTokenSource _cts;
        private bool _commandExecuting;

        public CancellationToken Token => _cts.Token;

        public CancelCommand(IScarletCommandManager commandManager)
        {
            _commandManager = commandManager ?? throw new ArgumentNullException(nameof(commandManager));
        }

        public void NotifyCommandStarting()
        {
            _commandExecuting = true;
            if (_cts?.IsCancellationRequested == false)
            {
                return;
            }

            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            RaiseCanExecuteChanged();
        }

        public void NotifyCommandFinished()
        {
            _commandExecuting = false;
            RaiseCanExecuteChanged();
        }

        bool ICommand.CanExecute(object parameter)
        {
            return _commandExecuting && _cts?.IsCancellationRequested != true;
        }

        void ICommand.Execute(object parameter)
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;

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
            _cts?.Dispose();
        }
    }
}
