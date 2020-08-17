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

        public AsyncCommand(in IScarletCommandManager commandManager, in Func<TArgument, CancellationToken, Task> methodToExecute)
        {
            _commandManager = commandManager ?? throw new ArgumentException($"{nameof(commandManager)} can't be empty.", nameof(commandManager));
            _cancelCommand = new CancelCommand(commandManager);

            _execute = methodToExecute ?? throw new ArgumentNullException($"{nameof(methodToExecute)} can't be empty.", nameof(methodToExecute));
            _canExecute = (_) => true;

            _execution = NotifyTaskCompletion.Completed;
        }

        public AsyncCommand(in IScarletCommandManager commandManager, in Func<TArgument, CancellationToken, Task> methodToExecute, in Func<TArgument, bool> canExecute)
        {
            _commandManager = commandManager ?? throw new ArgumentException($"{nameof(commandManager)} can't be empty.", nameof(commandManager));
            _cancelCommand = new CancelCommand(commandManager);

            _execute = methodToExecute ?? throw new ArgumentNullException($"{nameof(methodToExecute)} can't be empty.", nameof(methodToExecute));
            _canExecute = canExecute ?? throw new ArgumentException($"{nameof(canExecute)} can't be empty.", nameof(canExecute));

            _execution = NotifyTaskCompletion.Completed;
        }

        public async void Execute(object parameter)
        {
            await ExecuteAsync(parameter).ConfigureAwait(false);
        }

        public event EventHandler CanExecuteChanged
        {
            add { _commandManager.RequerySuggested += value; }
            remove { _commandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object? parameter)
        {
            var isRunning = Execution?.IsCompleted != false;

            if (parameter is null)
            {
                return isRunning && _canExecute.Invoke(default!);
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

            Execution = new NotifyTaskCompletion(_execute.Invoke(argument!, _cancelCommand.Token));

            RaiseCanExecuteChanged();

            await Execution.TaskCompletion.ConfigureAwait(false);
            _cancelCommand.NotifyCommandFinished();

            RaiseCanExecuteChanged();
        }

        private void OnPropertyChanged([CallerMemberName] in string? propertyName = null)
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
