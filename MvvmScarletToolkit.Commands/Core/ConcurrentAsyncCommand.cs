using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Commands
{
    /// <summary>
    /// provides a command that can cancel an already running command to start a new one
    /// </summary>
    internal sealed class ConcurrentAsyncCommand<TArgument, TResult> : GenericAsyncCommandBase<TResult>
    {
        private readonly Func<TArgument, CancellationToken, Task<TResult>> _command;
        private readonly CancelAsyncCommand _cancelCommand;
        private readonly Func<TArgument, bool> _canExecute;

        internal ConcurrentAsyncCommand(ICommandManager commandManager, Func<TArgument, CancellationToken, Task<TResult>> command)
            : base(commandManager)
        {
            _command = command ?? throw new ArgumentNullException(nameof(command));
            _cancelCommand = new CancelAsyncCommand(CommandManager);

            CancelCommand = _cancelCommand;
        }

        internal ConcurrentAsyncCommand(ICommandManager commandManager, Func<TArgument, CancellationToken, Task<TResult>> command, Func<TArgument, bool> canExecuteEvaluator)
            : this(commandManager, command)
        {
            _canExecute = canExecuteEvaluator ?? throw new ArgumentNullException(nameof(canExecuteEvaluator));
        }

        [DebuggerStepThrough]
        public override bool CanExecute(object parameter)
        {
            return CanExecuteInternal(parameter);
        }

        private bool CanExecuteInternal(object parameter)
        {
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
            if (IsBusy)
            {
                CancelCommand.Execute(parameter);
            }

            await ExecuteAsyncInternal(parameter).ConfigureAwait(false);
        }

        private async Task ExecuteAsyncInternal(object parameter)
        {
            var argument = parameter is TArgument arg
                ? arg
                : default;

            IsBusy = true;
            _cancelCommand.NotifyCommandStarting();

            try
            {
                Execution = new NotifyTaskCompletion<TResult>(_command(argument, _cancelCommand.Token));
                await Execution.TaskCompletion.ConfigureAwait(true);
            }
            finally
            {
                IsBusy = false;
                _cancelCommand.NotifyCommandFinished();

                RaiseCanExecuteChanged();
            }
        }
    }
}
