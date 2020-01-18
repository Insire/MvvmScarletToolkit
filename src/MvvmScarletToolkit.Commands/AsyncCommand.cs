using MvvmScarletToolkit.Abstractions;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MvvmScarletToolkit.Commands
{
    /// <summary>
    /// Task based ICommand implementation
    /// </summary>
    /// <typeparam name="TArgument">the argument that can be passed to <see cref="ICommand.Execute(object)"/> and <see cref="ICommand.CanExecute(object)"/></typeparam>
    public sealed class AsyncCommand<TArgument> : IAsyncCommand
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private readonly Func<TArgument, CancellationToken, Task>? _execute;
        private readonly Func<TArgument, bool>? _canExecute;

        private readonly IScarletCommandManager _commandManager;
        private readonly ICancelCommand _cancelCommand;

        private bool _disposed;

        public ICommand CancelCommand => _cancelCommand;

        private NotifyTaskCompletion? _execution;
        public NotifyTaskCompletion? Execution
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
            _commandManager = commandManager ?? throw new ArgumentException($"{nameof(commandManager)} can't be empty.", nameof(commandManager));
            _cancelCommand = new CancelCommand(commandManager);
        }

        public AsyncCommand(IScarletCommandManager commandManager, Func<TArgument, CancellationToken, Task> methodToExecute)
            : this(commandManager)
        {
            _execute = methodToExecute ?? throw new ArgumentNullException($"{nameof(methodToExecute)} can't be empty.", nameof(methodToExecute));
        }

        public AsyncCommand(IScarletCommandManager commandManager, Func<TArgument, CancellationToken, Task> command, Func<TArgument, bool> canExecute)
            : this(commandManager, command)
        {
            _canExecute = canExecute ?? throw new ArgumentException($"{nameof(canExecute)} can't be empty.", nameof(canExecute));
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

        public bool CanExecute(object? parameter)
        {
            var isRunning = Execution is null || Execution.IsCompleted;

            if (_canExecute is null)
            {
                return isRunning;
            }

            if (parameter is null)
            {
#pragma warning disable CS8653 // A default expression introduces a null value for a type parameter.
                return isRunning && _canExecute.Invoke(default);
#pragma warning restore CS8653 // A default expression introduces a null value for a type parameter.
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

            Execution = new NotifyTaskCompletion(_execute?.Invoke(argument, _cancelCommand.Token) ?? Task.CompletedTask);
            RaiseCanExecuteChanged();

            await Execution.TaskCompletion;
            _cancelCommand.NotifyCommandFinished();

            RaiseCanExecuteChanged();
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
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
