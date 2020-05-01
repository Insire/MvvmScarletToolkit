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
    public sealed class ConcurrentCommand<TArgument> : GenericConcurrentCommandBase
    {
        private readonly Func<TArgument, CancellationToken, Task> _execute;
        private readonly Func<TArgument, bool>? _canExecute;

        private readonly ICancelCommand _cancelCommand;
        private readonly IBusyStack? _externalBusyStack;
        private readonly IBusyStack _internalBusyStack;

        internal ConcurrentCommand(IScarletCommandManager commandManager, ICancelCommand cancelCommand, Func<Action<bool>, IBusyStack> busyStackFactory, Func<TArgument, CancellationToken, Task> execute)
            : base(commandManager)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            CancelCommand = _cancelCommand = cancelCommand ?? throw new ArgumentNullException(nameof(cancelCommand));

            _internalBusyStack = busyStackFactory?.Invoke(hasItems => IsBusy = hasItems) ?? throw new ArgumentNullException(nameof(busyStackFactory));
        }

        internal ConcurrentCommand(IScarletCommandManager commandManager, ICancelCommand cancelCommand, Func<Action<bool>, IBusyStack> busyStackFactory, Func<TArgument, CancellationToken, Task> command, Func<TArgument, bool> canExecuteEvaluator)
            : this(commandManager, cancelCommand, busyStackFactory, command)
        {
            _canExecute = canExecuteEvaluator ?? throw new ArgumentNullException(nameof(canExecuteEvaluator));
        }

        internal ConcurrentCommand(IScarletCommandManager commandManager, ICancelCommand cancelCommand, Func<Action<bool>, IBusyStack> busyStackFactory, IBusyStack externalBusyStack, Func<TArgument, CancellationToken, Task> command, Func<TArgument, bool> canExecuteEvaluator)
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
                return _canExecute.Invoke(default!);
            }

            return parameter is TArgument arg
                   && _canExecute.Invoke(arg);
        }

        [DebuggerStepThrough]
        public override async void Execute(object parameter)
        {
            await ExecuteAsync(parameter)
                .ConfigureAwait(false);
        }

        public override async Task ExecuteAsync(object parameter)
        {
            if (IsBusy)
            {
                CancelCommand?.Execute(parameter);
            }

            var argument = parameter is TArgument arg
                ? arg
                : default;

            if (_externalBusyStack is null)
            {
                await ExecuteWithoutBusyStack(argument!)
                    .ConfigureAwait(false);
            }
            else
            {
                await ExecuteWithBusyStack(argument!)
                    .ConfigureAwait(false);
            }
        }

        private async Task ExecuteWithBusyStack(TArgument parameter)
        {
            try
            {
                using (_externalBusyStack!.GetToken())
                {
                    await ExecuteAsyncInternal(parameter)
                        .ConfigureAwait(true); // return to UI thread here
                }
            }
            finally
            {
                _cancelCommand.NotifyCommandFinished();
                RaiseCanExecuteChanged();
            }
        }

        private async Task ExecuteWithoutBusyStack(TArgument parameter)
        {
            try
            {
                IsBusy = true;

                await ExecuteAsyncInternal(parameter)
                    .ConfigureAwait(true); // return to UI thread here
            }
            finally
            {
                IsBusy = false;

                RaiseCanExecuteChanged();
                _cancelCommand.NotifyCommandFinished();
            }
        }

        private async Task ExecuteAsyncInternal(TArgument parameter)
        {
            using (_internalBusyStack.GetToken())
            {
                _cancelCommand.NotifyCommandStarting();

                Execution = new NotifyTaskCompletion(_execute.Invoke(parameter, _cancelCommand.Token));
                await Execution.TaskCompletion
                    .ConfigureAwait(false);
            }
        }
    }
}
