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
        private readonly IScarletExceptionHandler _exceptionHandler;
        private readonly Func<TArgument, CancellationToken, Task> _execute;
        private readonly Func<TArgument, bool>? _canExecute;

        private readonly ICancelCommand _cancelCommand;
        private readonly IBusyStack? _externalBusyStack;
        private readonly IBusyStack _internalBusyStack;

        internal ConcurrentCommand(in IScarletCommandManager commandManager,
                                   in IScarletExceptionHandler exceptionHandler,
                                   in ICancelCommand cancelCommand,
                                   in Func<Action<bool>, IBusyStack> busyStackFactory,
                                   in Func<TArgument, CancellationToken, Task> execute)
            : base(commandManager)
        {
            _exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            CancelCommand = _cancelCommand = cancelCommand ?? throw new ArgumentNullException(nameof(cancelCommand));

            _internalBusyStack = busyStackFactory?.Invoke(hasItems => IsBusy = hasItems) ?? throw new ArgumentNullException(nameof(busyStackFactory));
        }

        internal ConcurrentCommand(in IScarletCommandManager commandManager,
                                   in IScarletExceptionHandler exceptionHandler,
                                   in ICancelCommand cancelCommand,
                                   in Func<Action<bool>, IBusyStack> busyStackFactory,
                                   in Func<TArgument, CancellationToken, Task> command,
                                   in Func<TArgument, bool> canExecuteEvaluator)
            : this(commandManager, exceptionHandler, cancelCommand, busyStackFactory, command)
        {
            _canExecute = canExecuteEvaluator ?? throw new ArgumentNullException(nameof(canExecuteEvaluator));
        }

        internal ConcurrentCommand(in IScarletCommandManager commandManager,
                                   in IScarletExceptionHandler exceptionHandler,
                                   in ICancelCommand cancelCommand,
                                   in Func<Action<bool>, IBusyStack> busyStackFactory,
                                   in IBusyStack externalBusyStack,
                                   in Func<TArgument, CancellationToken, Task> command,
                                   in Func<TArgument, bool> canExecuteEvaluator)
            : this(commandManager, exceptionHandler, cancelCommand, busyStackFactory, command, canExecuteEvaluator)
        {
            _externalBusyStack = externalBusyStack ?? throw new ArgumentNullException(nameof(externalBusyStack));
        }

        [DebuggerStepThrough]
        public override bool CanExecute(object? parameter)
        {
            return CanExecuteInternal(parameter);
        }

        private bool CanExecuteInternal(object? parameter)
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
        public override async void Execute(object? parameter)
        {
            await ExecuteAsync(parameter)
                .ConfigureAwait(false);
        }

        public override async Task ExecuteAsync(object? parameter)
        {
            if (IsBusy)
            {
                CancelCommand?.Execute(parameter);
            }

            var argument = parameter is TArgument arg
                ? arg
                : default;

            var externalToken = default(IDisposable);
            try
            {
                externalToken = _externalBusyStack?.GetToken();

                using (_internalBusyStack.GetToken())
                {
                    try
                    {
                        _cancelCommand.NotifyCommandStarting();

                        // if we intend to react with a binding to the currently running state of the command, we need to tell it to rerun CanExecute
                        CommandManager.InvalidateRequerySuggested();

                        Execution = new NotifyTaskCompletion(_execute.Invoke(argument!, _cancelCommand.Token), _exceptionHandler);
                        await Execution.TaskCompletion.ConfigureAwait(true); // return to UI thread here
                    }
                    finally
                    {
                        _cancelCommand.NotifyCommandFinished();
                    }
                }
            }
            finally
            {
                externalToken?.Dispose();

                RaiseCanExecuteChanged();
            }
        }
    }
}
