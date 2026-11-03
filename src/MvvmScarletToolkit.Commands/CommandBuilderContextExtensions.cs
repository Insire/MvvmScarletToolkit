using MvvmScarletToolkit.Commands;

namespace MvvmScarletToolkit
{
    public static class CommandBuilderContextExtensions
    {
        /// <summary>
        /// Configure synchronous cancellation for the given command
        /// </summary>
        public static CommandBuilderContext<TArgument> WithCancellation<TArgument>(this CommandBuilderContext<TArgument> context)
        {
            ValdiateContextForNull(context);

            var commandManager = context.CommandManager;
            context.CancelCommandFactory = () => new CancelCommand(commandManager);
            return context;
        }

        /// <summary>
        /// Configure task based cancellation for the given command
        /// </summary>
        public static CommandBuilderContext<TArgument> WithAsyncCancellation<TArgument>(this CommandBuilderContext<TArgument> context)
        {
            ValdiateContextForNull(context);

            var commandManager = context.CommandManager;
            var exceptionHandler = context.ExceptionHandler;
            context.CancelCommandFactory = () => new ConcurrentCancelCommand(commandManager, exceptionHandler);
            return context;
        }

        /// <summary>
        /// Provide a custom implementation of <see cref="ICancelCommand"/>
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        public static CommandBuilderContext<TArgument> WithCustomCancellation<TArgument>(this CommandBuilderContext<TArgument> context, in ICancelCommand cancelCommand)
        {
            ValdiateContextForNull(context);

            if (cancelCommand is null)
            {
                throw new ArgumentNullException(nameof(cancelCommand), "This method should be called with a non null instance of ICancelCommand.");
            }

            // the caller provides the instance, so it is shared between all commands built from this context
            var instance = cancelCommand;
            context.CancelCommandFactory = () => instance;
            return context;
        }

        /// <summary>
        /// Provide a custom implementation of <see cref="IBusyStack"/>
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        public static CommandBuilderContext<TArgument> WithBusyNotification<TArgument>(this CommandBuilderContext<TArgument> context, in IBusyStack busyStack)
        {
            ValdiateContextForNull(context);

            if (busyStack is null)
            {
                throw new ArgumentNullException(nameof(busyStack), "This method should be called with a non null instance of IBusyStack.");
            }

            context.BusyStack = busyStack;
            return context;
        }

        /// <summary>
        /// Configure a command as to not automatically cancel and rerun when executed again.
        /// Which means there will be only one command be startable at a time.
        /// </summary>
        public static CommandBuilderContext<TArgument> WithSingleExecution<TArgument>(this CommandBuilderContext<TArgument> context)
        {
            return context.WithSingleExecution(default);
        }

        /// <summary>
        /// Configure a command as to not automatically cancel and rerun when executed again.
        /// </summary>
        /// <param name="context">the context to configure</param>
        /// <param name="name">identifies the command in debug output</param>
        public static CommandBuilderContext<TArgument> WithSingleExecution<TArgument>(this CommandBuilderContext<TArgument> context, string? name)
        {
            return context.WithSingleExecution(name, SequentialAsyncCommandDecorator.DefaultRunningReason);
        }

        /// <summary>
        /// Configure a command as to not automatically cancel and rerun when executed again.
        /// </summary>
        /// <param name="context">the context to configure</param>
        /// <param name="name">identifies the command in debug output</param>
        /// <param name="runningReason">reported as <see cref="ConcurrentCommandBase.DisabledReason"/> while the command is running, <see langword="null"/> reports nothing</param>
        public static CommandBuilderContext<TArgument> WithSingleExecution<TArgument>(this CommandBuilderContext<TArgument> context, string? name, string? runningReason)
        {
            ValdiateContextForNull(context);

            ConcurrentCommandBase DecoratorFactory(ConcurrentCommandBase command)
            {
                return new SequentialAsyncCommandDecorator(context.CommandManager, command, name, runningReason);
            }

            context.AddDecorator(DecoratorFactory);

            return context;
        }

        /// <summary>
        /// Explain why the command cannot execute. The evaluator is only invoked when <c>canExecute</c> returned <see langword="false"/>,
        /// and its result is exposed through <see cref="ConcurrentCommandBase.DisabledReason"/>.
        /// </summary>
        public static CommandBuilderContext<TArgument> WithDisabledReason<TArgument>(this CommandBuilderContext<TArgument> context, Func<string?> disabledReason)
        {
            if (disabledReason is null)
            {
                throw new ArgumentNullException(nameof(disabledReason));
            }

            return context.WithDisabledReason(_ => disabledReason());
        }

        /// <summary>
        /// Explain why the command cannot execute. The evaluator is only invoked when <c>canExecute</c> returned <see langword="false"/>,
        /// and its result is exposed through <see cref="ConcurrentCommandBase.DisabledReason"/>.
        /// </summary>
        public static CommandBuilderContext<TArgument> WithDisabledReason<TArgument>(this CommandBuilderContext<TArgument> context, Func<TArgument, string?> disabledReason)
        {
            ValdiateContextForNull(context);

            context.DisabledReasonEvaluator = disabledReason ?? throw new ArgumentNullException(nameof(disabledReason));
            return context;
        }

        private static void ValdiateContextForNull<TArgument>(CommandBuilderContext<TArgument> context)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context), "This method should be called on a non null instance of CommandBuilderContext<TArgument>.");
            }
        }
    }
}
