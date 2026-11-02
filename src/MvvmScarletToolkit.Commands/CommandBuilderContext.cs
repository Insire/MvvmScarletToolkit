using System.ComponentModel;

namespace MvvmScarletToolkit.Commands
{
    /// <summary>
    /// Context class for creating and configuring <see cref="ConcurrentCommand{TArgument}"/> via the fluent builder pattern
    /// </summary>
    /// <typeparam name="TArgument">The argument type, that the to be created command is supposed to accept</typeparam>
    [Bindable(false)]
    public sealed class CommandBuilderContext<TArgument> : AbstractBuilder<ConcurrentCommandBase>
    {
        private readonly List<Func<ConcurrentCommandBase, ConcurrentCommandBase>> _decorators;
        private readonly Func<Action<bool>, IBusyStack> _busyStackFactory;

        private readonly Func<TArgument, CancellationToken, Task> _execute;
        private readonly Func<TArgument, bool> _canExecute;

        internal IScarletCommandManager CommandManager { get; }
        internal IScarletExceptionHandler ExceptionHandler { get; }

        /// <summary>
        /// optional factory for an <see cref="System.Windows.Input.ICommand"/> for Cancellation-Support.
        /// A factory is used, so that every call to <see cref="Build"/> creates a command with its own cancellation state
        /// </summary>
        internal Func<ICancelCommand>? CancelCommandFactory { get; set; }

        /// <summary>
        /// optional external <see cref="IBusyStack"/> that can be hooked up to receive notifications from the internal busy state
        /// </summary>
        internal IBusyStack? BusyStack { get; set; }

        public CommandBuilderContext(in IScarletCommandManager commandManager,
                                     in IScarletExceptionHandler exceptionHandler,
                                     in Func<Action<bool>, IBusyStack> busyStackFactory,
                                     in Func<TArgument, CancellationToken, Task> execute,
                                     in Func<TArgument, bool> canExecute)
        {
            CommandManager = commandManager ?? throw new ArgumentNullException(nameof(commandManager));
            ExceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));

            _decorators = new List<Func<ConcurrentCommandBase, ConcurrentCommandBase>>();
            _busyStackFactory = busyStackFactory ?? throw new ArgumentNullException(nameof(busyStackFactory));

            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute ?? throw new ArgumentNullException(nameof(canExecute));
        }

        internal void AddDecorator(in Func<ConcurrentCommandBase, ConcurrentCommandBase> decoratorFactory)
        {
            if (decoratorFactory is null)
            {
                throw new ArgumentNullException(nameof(decoratorFactory));
            }

            _decorators.Add(decoratorFactory);
        }

        public override ConcurrentCommandBase Build()
        {
            var command = CreateCommand();

            return WrapInDecorators(command);
        }

        private ConcurrentCommand<TArgument> CreateCommand()
        {
            var cancelCommand = CancelCommandFactory?.Invoke() ?? NoCancellationCommand.Default;

            if (BusyStack is null)
            {
                return new ConcurrentCommand<TArgument>(CommandManager, ExceptionHandler, cancelCommand, _busyStackFactory, _execute, _canExecute);
            }

            return new ConcurrentCommand<TArgument>(CommandManager, ExceptionHandler, cancelCommand, _busyStackFactory, BusyStack, _execute, _canExecute);
        }

        private ConcurrentCommandBase WrapInDecorators(in ConcurrentCommandBase command)
        {
            // the decorators are not consumed, so that Build (and the implicit conversion) can be called more than once
            var decorator = command;
            foreach (var wrapper in _decorators)
            {
                decorator = wrapper.Invoke(decorator);
            }

            return decorator;
        }
    }
}
