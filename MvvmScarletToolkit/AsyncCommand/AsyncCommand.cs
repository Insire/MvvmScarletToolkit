using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MvvmScarletToolkit.Commands
{
    // source: https://msdn.microsoft.com/en-us/magazine/dn630647.aspx?f=255&MSPPError=-2147217396
    // Async Programming : Patterns for Asynchronous MVVM Applications: Commands by Stephen Cleary
    public sealed class AsyncCommand<TArgument> : AsyncCommandBase
    {
        private readonly Func<TArgument, CancellationToken, Task> _execute;
        private readonly CancelAsyncCommand _cancelCommand;
        private readonly Func<TArgument, bool> _canExecute = null;

        private bool _disposed;

        public ICommand CancelCommand => _cancelCommand;

        private NotifyTaskCompletion _execution;
        public NotifyTaskCompletion Execution
        {
            get { return _execution; }
            private set
            {
                if (_execution != value)
                {
                    _execution = value;

                    OnPropertyChanged();
                }
            }
        }

        private AsyncCommand()
        {
            _cancelCommand = new CancelAsyncCommand();
        }

        public AsyncCommand(Func<TArgument, CancellationToken, Task> execute)
            : this()
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        }

        public AsyncCommand(Func<TArgument, CancellationToken, Task> command, Func<TArgument, bool> canExecute)
            : this(command)
        {
            _canExecute = canExecute ?? throw new ArgumentNullException(nameof(canExecute));
        }

        public override bool CanExecute(object parameter)
        {
            var isRunning = Execution is null || Execution.IsCompleted;

            if (_canExecute is null)
            {
                return isRunning;
            }

            if (parameter is null)
            {
                return isRunning && _canExecute.Invoke(default);
            }

            return isRunning
                && parameter is TArgument argument
                && _canExecute.Invoke(argument);
        }

        public override async Task ExecuteAsync(object parameter)
        {
            _cancelCommand.NotifyCommandStarting();

            var argument = parameter is TArgument arg
                ? arg
                : default;

            Execution = new NotifyTaskCompletion(_execute(argument, _cancelCommand.Token));
            RaiseCanExecuteChanged();

            await Execution.TaskCompletion.ConfigureAwait(true);
            _cancelCommand.NotifyCommandFinished();
            RaiseCanExecuteChanged();
        }

        public override void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _cancelCommand.Dispose();

            _disposed = true;
        }

        private sealed class CancelAsyncCommand : ICommand, IDisposable
        {
            private CancellationTokenSource _cts;
            private bool _commandExecuting;
            private bool _disposed;

            public CancellationToken Token => _cts.Token;

            public void NotifyCommandStarting()
            {
                _commandExecuting = true;

                if (!_cts?.IsCancellationRequested == true)
                    return;

                _cts?.Cancel();
                _cts?.Dispose();
                _cts = new CancellationTokenSource();

                RaiseCanExecuteChanged();
            }

            public void NotifyCommandFinished()
            {
                _commandExecuting = false;

                RaiseCanExecuteChanged();

                _cts?.Cancel();
                _cts?.Dispose();
                _cts = null;
            }

            bool ICommand.CanExecute(object parameter)
            {
                return _commandExecuting && !_cts?.IsCancellationRequested == true;
            }

            void ICommand.Execute(object parameter)
            {
                _cts?.Cancel();
                RaiseCanExecuteChanged();
            }

            public event EventHandler CanExecuteChanged
            {
                add { CommandManager.RequerySuggested += value; }
                remove { CommandManager.RequerySuggested -= value; }
            }

            public void Dispose()
            {
                if (_disposed)
                {
                    return;
                }

                _cts?.Cancel();
                _cts?.Dispose();
                _cts = null;

                _disposed = true;
            }

            private void RaiseCanExecuteChanged()
            {
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }

    public static class AsyncCommand
    {
        public static IAsyncCommand Create(Func<Task> command)
        {
            return new AsyncCommand<object>((parameter, token) => command());
        }

        public static IAsyncCommand Create(Func<Task> command, Func<bool> canExecute)
        {
            return new AsyncCommand<object>((parameter, token) => command(), (parameter) => canExecute());
        }

        public static IAsyncCommand Create<TArgument>(Func<Task<TArgument>> command, Func<bool> canExecute)
        {
            return new AsyncCommand<TArgument>((parameter, token) => command(), (parameter) => canExecute());
        }

        public static IAsyncCommand Create(Func<CancellationToken, Task> command, Func<bool> canExecute)
        {
            return new AsyncCommand<object>((parameter, token) => command(token), (parameter) => canExecute());
        }

        public static IAsyncCommand Create<TArgument>(Func<CancellationToken, Task> command, Func<bool> canExecute)
        {
            return new AsyncCommand<TArgument>((parameter, token) => command(token), (parameter) => canExecute());
        }

        public static IAsyncCommand Create<TArgument>(Func<TArgument, CancellationToken, Task> command, Func<TArgument, bool> canExecute)
        {
            return new AsyncCommand<TArgument>((parameter, token) => command(parameter, token), (parameter) => canExecute(parameter));
        }
    }
}
