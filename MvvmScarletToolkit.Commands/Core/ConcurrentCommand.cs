using MvvmScarletToolkit.Abstractions;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Commands
{
    /// <summary>
    /// provides a command that can cancel an already running command to start a new one
    /// </summary>
    public sealed class ConcurrentCommand<TArgument, TResult> : GenericConcurrentCommandBase<TResult>
    {
        private readonly Func<TArgument, CancellationToken, Task<TResult>> _command;
        private readonly ICancelCommand _cancelCommand;
        private readonly Func<TArgument, bool> _canExecute;
        private readonly IBusyStack _externalBusyStack;
        private readonly IBusyStack _internalBusyStack;

        internal ConcurrentCommand(IScarletCommandManager commandManager, ICancelCommand cancelCommand, Func<Action<bool>, IBusyStack> busyStackFactory, Func<TArgument, CancellationToken, Task<TResult>> command)
            : base(commandManager)
        {
            _command = command ?? throw new ArgumentNullException(nameof(command));
            CancelCommand = _cancelCommand = cancelCommand ?? throw new ArgumentNullException(nameof(cancelCommand));

            _internalBusyStack = busyStackFactory?.Invoke(hasItems => IsBusy = hasItems) ?? throw new ArgumentNullException(nameof(busyStackFactory));
        }

        internal ConcurrentCommand(IScarletCommandManager commandManager, ICancelCommand cancelCommand, Func<Action<bool>, IBusyStack> busyStackFactory, Func<TArgument, CancellationToken, Task<TResult>> command, Func<TArgument, bool> canExecuteEvaluator)
            : this(commandManager, cancelCommand, busyStackFactory, command)
        {
            _canExecute = canExecuteEvaluator ?? throw new ArgumentNullException(nameof(canExecuteEvaluator));
        }

        internal ConcurrentCommand(IScarletCommandManager commandManager, ICancelCommand cancelCommand, Func<Action<bool>, IBusyStack> busyStackFactory, IBusyStack externalBusyStack, Func<TArgument, CancellationToken, Task<TResult>> command, Func<TArgument, bool> canExecuteEvaluator)
            : this(commandManager, cancelCommand, busyStackFactory, command, canExecuteEvaluator)
        {
            _externalBusyStack = externalBusyStack ?? throw new ArgumentNullException(nameof(externalBusyStack));
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

            if (_externalBusyStack is null)
            {
                await ExecuteWithoutBusyStack(parameter).ConfigureAwait(false);
            }
            else
            {
                await ExecuteWithBusyStack(parameter).ConfigureAwait(false);
            }
        }

        private async Task ExecuteWithBusyStack(object parameter)
        {
            using (_externalBusyStack.GetToken())
            {
                await ExecuteAsyncInternal(parameter).ConfigureAwait(false);
            }
        }

        private async Task ExecuteWithoutBusyStack(object parameter)
        {
            try
            {
                IsBusy = true;
                await ExecuteAsyncInternal(parameter).ConfigureAwait(false);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task ExecuteAsyncInternal(object parameter)
        {
            var argument = parameter is TArgument arg
                ? arg
                : default;

            using (_internalBusyStack.GetToken())
            {
                _cancelCommand.NotifyCommandStarting();

                try
                {
                    Execution = new NotifyTaskCompletion<TResult>(_command.Invoke(argument, _cancelCommand.Token));
                    await Execution.TaskCompletion.ConfigureAwait(true);
                }
                finally
                {
                    _cancelCommand.NotifyCommandFinished();

                    RaiseCanExecuteChanged();
                }
            }
        }
    }
}
