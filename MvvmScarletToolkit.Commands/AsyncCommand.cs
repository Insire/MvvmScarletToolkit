using MvvmScarletToolkit.Abstractions;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MvvmScarletToolkit.Commands
{
    // source: https://msdn.microsoft.com/en-us/magazine/dn630647.aspx?f=255&MSPPError=-2147217396
    // Async Programming : Patterns for Asynchronous MVVM Applications: Commands by Stephen Cleary
    public sealed class AsyncCommand<TArgument> : IAsyncCommand
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly Func<TArgument, CancellationToken, Task> _execute;
        private readonly Func<TArgument, bool> _canExecute;

        private readonly IScarletCommandManager _commandManager;
        private readonly ICancelCommand _cancelCommand;

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

        private AsyncCommand(IScarletCommandManager commandManager)
        {
            _commandManager = commandManager ?? throw new ArgumentNullException(nameof(commandManager));
            _cancelCommand = new CancelCommand(commandManager);
        }

        public AsyncCommand(IScarletCommandManager commandManager, Func<TArgument, CancellationToken, Task> execute)
            : this(commandManager)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        }

        public AsyncCommand(IScarletCommandManager commandManager, Func<TArgument, CancellationToken, Task> command, Func<TArgument, bool> canExecute)
            : this(commandManager, command)
        {
            _canExecute = canExecute ?? throw new ArgumentNullException(nameof(canExecute));
        }

        public async void Execute(object parameter)
        {
            await ExecuteAsync(parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { _commandManager.RequerySuggested += value; }
            remove { _commandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
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

        public async Task ExecuteAsync(object parameter)
        {
            _cancelCommand.NotifyCommandStarting();

            var argument = parameter is TArgument arg
                ? arg
                : default;

            Execution = new NotifyTaskCompletion(_execute(argument, _cancelCommand.Token));
            RaiseCanExecuteChanged();

            await Execution.TaskCompletion;
            _cancelCommand.NotifyCommandFinished();

            RaiseCanExecuteChanged();
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void RaiseCanExecuteChanged()
        {
            _commandManager.InvalidateRequerySuggested();
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _cancelCommand.Dispose();

            _disposed = true;
        }
    }
}
