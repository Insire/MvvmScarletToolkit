using MvvmScarletToolkit.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Commands
{
    public static class CommandBuilderContextExtensions
    {
        /// <summary>
        /// Configure synchronous cancellation for the given command
        /// </summary>
        /// <param name="commandBuilder"></param>
        /// <returns></returns>
        public static ICommandBuilderContext WithCancellation(this ICommandBuilderContext commandBuilder)
        {
            commandBuilder.CancelCommand = new CancelCommand(commandBuilder.CommandManager);
            return commandBuilder;
        }

        /// <summary>
        /// Configure asynchronous cancellation for the given command
        /// </summary>
        /// <param name="commandBuilder"></param>
        /// <returns></returns>
        public static ICommandBuilderContext WithAsyncCancellation(this ICommandBuilderContext commandBuilder)
        {
            throw new NotImplementedException(); // TODO add implementation
        }

        public static ICommandBuilderContext WithBusyNotification(this ICommandBuilderContext commandBuilder)
        {
            throw new NotImplementedException(); // TODO add implementation
        }

        /// <summary>
        /// Configure a command as to not automatically cancel and rerun when executed again.
        /// Which means there will be only one command be startable at a time.
        /// </summary>
        /// <param name="commandBuilder"></param>
        /// <returns></returns>
        public static ICommandBuilderContext WithSingleExecution(this ICommandBuilderContext commandBuilder)
        {
            IExtendedAsyncCommand DecoratorFactory(IExtendedAsyncCommand command)
            {
                return new SequentialAsyncCommandDecorator(command);
            }

            commandBuilder.AddDecorator(DecoratorFactory);

            return commandBuilder;
        }

        public static CommandBuilderContext<TArgument, TResult> Create<TArgument, TResult>(this CommandBuilder builder, Func<Task> execute)
        {
            async Task<TResult> ExecuteWrapper(TArgument _, CancellationToken __)
            {
                await execute().ConfigureAwait(false);
                return default;
            }

            return builder.Create<TArgument, TResult>(ExecuteWrapper);
        }

        public static CommandBuilderContext<object, object> Create(this CommandBuilder builder, Func<Task> execute)
        {
            async Task<object> ExecuteWrapper(object _, CancellationToken __)
            {
                await execute().ConfigureAwait(false);
                return default;
            }

            return builder.Create<object, object>(ExecuteWrapper);
        }

        public static CommandBuilderContext<TArgument, TResult> Create<TArgument, TResult>(this CommandBuilder builder, Func<Task> execute, Func<bool> canExecute)
        {
            async Task<TResult> ExecuteWrapper(TArgument _, CancellationToken __)
            {
                await execute().ConfigureAwait(false);
                return default;
            }

            return builder.Create<TArgument, TResult>(ExecuteWrapper, _ => canExecute());
        }

        public static CommandBuilderContext<TArgument, TResult> Create<TArgument, TResult>(this CommandBuilder builder, Func<object, Task> execute, Func<bool> canExecute)
        {
            async Task<TResult> ExecuteWrapper(TArgument _, CancellationToken token)
            {
                await execute(token).ConfigureAwait(false);
                return default;
            }
            return builder.Create<TArgument, TResult>(ExecuteWrapper, _ => canExecute());
        }

        public static CommandBuilderContext<TArgument, object> Create<TArgument>(this CommandBuilder builder, Func<object, Task> execute, Func<bool> canExecute)
        {
            async Task<object> ExecuteWrapper(TArgument _, CancellationToken token)
            {
                await execute(token).ConfigureAwait(false);
                return default;
            }

            return builder.Create<TArgument, object>(ExecuteWrapper, _ => canExecute());
        }

        public static CommandBuilderContext<TArgument, object> Create<TArgument>(this CommandBuilder builder, Func<TArgument, Task> execute, Func<TArgument, bool> canExecute)
        {
            async Task<object> ExecuteWrapper(TArgument arg, CancellationToken _)
            {
                await execute(arg).ConfigureAwait(false);
                return default;
            }

            return builder.Create(ExecuteWrapper, canExecute);
        }

        public static CommandBuilderContext<object, TResult> Create<TResult>(this CommandBuilder builder, Func<Task<TResult>> execute, Func<bool> canExecute)
        {
            async Task<TResult> ExecuteWrapper(object _, CancellationToken __)
            {
                return await execute().ConfigureAwait(false);
            }

            return builder.Create<object, TResult>(ExecuteWrapper, _ => canExecute());
        }

        public static CommandBuilderContext<object, TResult> Create<TResult>(this CommandBuilder builder, Func<CancellationToken, Task<TResult>> execute, Func<bool> canExecute)
        {
            async Task<TResult> ExecuteWrapper(object _, CancellationToken token)
            {
                return await execute(token).ConfigureAwait(false);
            }

            return builder.Create<object, TResult>(ExecuteWrapper, _ => canExecute());
        }

        public static CommandBuilderContext<TArgument, TResult> Create<TArgument, TResult>(this CommandBuilder builder, Func<TArgument, CancellationToken, Task<TResult>> execute, Func<TArgument, bool> canExecute)
        {
            return builder.Create(execute, canExecute);
        }

        public static CommandBuilderContext<object, object> Create(this CommandBuilder builder, Func<CancellationToken, Task> execute, Func<bool> canExecute)
        {
            async Task<object> ExecuteWrapper(object _, CancellationToken token)
            {
                await execute(token).ConfigureAwait(false);
                return default;
            }

            return builder.Create<object, object>(ExecuteWrapper, _ => canExecute());
        }

        public static CommandBuilderContext<object, object> Create(this CommandBuilder builder, Func<object, Task> execute, Func<bool> canExecute)
        {
            async Task<object> ExecuteWrapper(object _, CancellationToken token)
            {
                await execute(token).ConfigureAwait(false);
                return default;
            }

            return builder.Create<object, object>(ExecuteWrapper, _ => canExecute());
        }

        public static CommandBuilderContext<object, object> Create(this CommandBuilder builder, Func<Task> execute, Func<bool> canExecute)
        {
            async Task<object> ExecuteWrapper(object _, CancellationToken __)
            {
                await execute().ConfigureAwait(false);
                return default;
            }

            return builder.Create<object, object>(ExecuteWrapper, _ => canExecute());
        }
    }
}
