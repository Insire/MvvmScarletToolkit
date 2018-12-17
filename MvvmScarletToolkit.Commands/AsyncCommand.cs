using MvvmScarletToolkit.Abstractions;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MvvmScarletToolkit.Commands
{
    // source: https://msdn.microsoft.com/en-us/magazine/dn630647.aspx?f=255&MSPPError=-2147217396
    // Async Programming : Patterns for Asynchronous MVVM Applications: Commands by Stephen Cleary
    public sealed class AsyncCommand<TArgument, TResult> : AsyncCommandBase, IExtendedAsyncCommand
    {
        private readonly Func<TArgument, CancellationToken, Task<TResult>> _command;
        private readonly CancelAsyncCommand _cancelCommand;
        private readonly Func<TArgument, bool> _canExecute;

        [Bindable(true, BindingDirection.OneWay)]
        public ICommand CancelCommand => _cancelCommand;

        private NotifyTaskCompletion<TResult> _execution;
        [Bindable(true, BindingDirection.OneWay)]
        public NotifyTaskCompletion<TResult> Execution
        {
            get { return _execution; }
            private set { SetValue(ref _execution, value); }
        }

        public AsyncCommand(Func<TArgument, CancellationToken, Task<TResult>> command)
        {
            _command = command ?? throw new ArgumentNullException(nameof(command));
            _cancelCommand = new CancelAsyncCommand();
        }

        public AsyncCommand(Func<TArgument, CancellationToken, Task<TResult>> command, Func<TArgument, bool> canExecuteEvaluator)
            : this(command)
        {
            _canExecute = canExecuteEvaluator ?? throw new ArgumentNullException(nameof(canExecuteEvaluator));
        }

        public override bool CanExecute(object parameter)
        {
            return CanExecuteInternal(parameter);
        }

        private bool CanExecuteInternal(object parameter)
        {
            if (IsBusy)
            {
                return false;
            }

            if (Execution?.IsCompleted == false)
            {
                return false;
            }

            if (_canExecute is null)
            {
                return true;
            }

            if (parameter is null)
            {
                return _canExecute.Invoke(default);
            }

            return parameter is TArgument arg
                && _canExecute.Invoke(arg);
        }

        [DebuggerStepThrough]
        public override async void Execute(object parameter)
        {
            await ExecuteAsync(parameter).ConfigureAwait(false);
        }

        public override async Task ExecuteAsync(object parameter)
        {
            var arg = parameter is TArgument
                ? (TArgument)parameter
                : default;

            IsBusy = true;
            _cancelCommand.NotifyCommandStarting();

            try
            {
                Execution = new NotifyTaskCompletion<TResult>(_command(arg, _cancelCommand.Token));
                await Execution.TaskCompletion.ConfigureAwait(true);
            }
            finally
            {
                IsBusy = false;
                _cancelCommand.NotifyCommandFinished();

                RaiseCanExecuteChanged();
            }
        }

        private sealed class CancelAsyncCommand : ICommand
        {
            private CancellationTokenSource _cts = new CancellationTokenSource();
            private bool _commandExecuting;

            public CancellationToken Token => _cts.Token;

            public void NotifyCommandStarting()
            {
                _commandExecuting = true;
                if (!_cts.IsCancellationRequested)
                {
                    return;
                }

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
                return _commandExecuting && !_cts.IsCancellationRequested;
            }

            void ICommand.Execute(object parameter)
            {
                _cts.Cancel();
                RaiseCanExecuteChanged();
            }

            public event EventHandler CanExecuteChanged
            {
                add { CommandManager.RequerySuggested += value; }
                remove { CommandManager.RequerySuggested -= value; }
            }

            private void RaiseCanExecuteChanged()
            {
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }

    public static class AsyncCommand
    {
        public static AsyncCommand<object, object> Create(Func<Task> command)
        {
            return new AsyncCommand<object, object>(async (_, __) =>
            {
                await command().ConfigureAwait(false);
                return null;
            });
        }

        public static AsyncCommand<object, object> Create(Func<Task> command, Func<bool> canExecute)
        {
            return new AsyncCommand<object, object>(async (_, __) =>
            {
                await command().ConfigureAwait(false);
                return null;
            }, _ => canExecute());
        }

        public static AsyncCommand<object, object> Create(Func<object, Task> command, Func<bool> canExecute)
        {
            return new AsyncCommand<object, object>(async (_, token) =>
            {
                await command(token).ConfigureAwait(false);
                return null;
            }, _ => canExecute());
        }

        public static AsyncCommand<object, object> Create(Func<CancellationToken, Task> command, Func<bool> canExecute)
        {
            return new AsyncCommand<object, object>(async (_, token) =>
            {
                await command(token).ConfigureAwait(false);
                return null;
            }, _ => canExecute());
        }

        public static AsyncCommand<TArgument, object> Create<TArgument>(Func<TArgument, CancellationToken, Task> command, Func<TArgument, bool> canExecute)
        {
            return new AsyncCommand<TArgument, object>(async (arg, token) =>
            {
                await command(arg, token).ConfigureAwait(false);
                return null;
            }, (arg) => canExecute(arg));
        }

        public static AsyncCommand<object, TResult> Create<TResult>(Func<Task<TResult>> command, Func<bool> canExecute)
        {
            return new AsyncCommand<object, TResult>(async (_, __) => await command().ConfigureAwait(false), _ => canExecute());
        }

        public static AsyncCommand<object, TResult> Create<TResult>(Func<CancellationToken, Task<TResult>> command, Func<bool> canExecute)
        {
            return new AsyncCommand<object, TResult>(async (_, token) => await command(token).ConfigureAwait(false), _ => canExecute());
        }

        public static AsyncCommand<TArgument, TResult> Create<TArgument, TResult>(Func<TArgument, CancellationToken, Task<TResult>> command, Func<TArgument, bool> canExecute)
        {
            return new AsyncCommand<TArgument, TResult>(command, arg => canExecute(arg));
        }
    }
}
