using MvvmScarletToolkit.Abstractions;
using MvvmScarletToolkit.Commands;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit
{
    public static class CommandBuilderContextExtensions
    {
        public static CommandBuilderContext<object> Create(this ICommandBuilder builder, Func<Task> execute)
        {
            return builder.Create<object>((parameter, token) => execute(), (parameter) => true);
        }

        public static CommandBuilderContext<object> Create(this ICommandBuilder builder, Func<Task> execute, Func<bool> canExecute)
        {
            return builder.Create<object>((parameter, token) => execute(), (parameter) => canExecute());
        }

        public static CommandBuilderContext<object> Create(this ICommandBuilder builder, Func<CancellationToken, Task> execute, Func<bool> canExecute)
        {
            return builder.Create<object>((parameter, token) => execute(token), (parameter) => canExecute());
        }

        public static CommandBuilderContext<TArgument> Create<TArgument>(this ICommandBuilder builder, Func<Task> execute, Func<bool> canExecute)
        {
            return builder.Create<TArgument>((parameter, token) => execute(), (parameter) => canExecute());
        }

        public static CommandBuilderContext<TArgument> Create<TArgument>(this ICommandBuilder builder, Func<CancellationToken, Task> execute, Func<bool> canExecute)
        {
            return builder.Create<TArgument>((parameter, token) => execute(token), (parameter) => canExecute());
        }

        public static CommandBuilderContext<TArgument> Create<TArgument>(this ICommandBuilder builder, Func<TArgument, Task> execute, Func<bool> canExecute)
        {
            return builder.Create<TArgument>((parameter, token) => execute(parameter), (parameter) => canExecute());
        }

        public static CommandBuilderContext<TArgument> Create<TArgument>(this ICommandBuilder builder, Func<TArgument, Task> execute, Func<TArgument, bool> canExecute)
        {
            return builder.Create<TArgument>((parameter, token) => execute(parameter), (parameter) => canExecute(parameter));
        }

        public static CommandBuilderContext<TArgument> Create<TArgument>(this ICommandBuilder builder, Func<TArgument, CancellationToken, Task> execute, Func<TArgument, bool> canExecute)
        {
            return builder.Create<TArgument>((parameter, token) => execute(parameter, token), (parameter) => canExecute(parameter));
        }

        /// <summary>
        /// Configure synchronous cancellation for the given command
        /// </summary>
        /// <param name="commandBuilder"></param>
        public static CommandBuilderContext<TArgument> WithCancellation<TArgument>(this CommandBuilderContext<TArgument> commandBuilder)
        {
            commandBuilder.CancelCommand = new CancelCommand(commandBuilder.CommandManager);
            return commandBuilder;
        }

        /// <summary>
        /// Configure asynchronous cancellation for the given command
        /// </summary>
        /// <param name="commandBuilder"></param>
        public static CommandBuilderContext<TArgument> WithAsyncCancellation<TArgument>(this CommandBuilderContext<TArgument> commandBuilder)
        {
            commandBuilder.CancelCommand = new ConcurrentCancelCommand(commandBuilder.CommandManager);
            return commandBuilder;
        }

        public static CommandBuilderContext<TArgument> WithBusyNotification<TArgument>(this CommandBuilderContext<TArgument> commandBuilder, IBusyStack busyStack)
        {
            commandBuilder.BusyStack = busyStack;
            return commandBuilder;
        }

        /// <summary>
        /// Configure a command as to not automatically cancel and rerun when executed again.
        /// Which means there will be only one command be startable at a time.
        /// </summary>
        /// <param name="commandBuilder"></param>
        public static CommandBuilderContext<TArgument> WithSingleExecution<TArgument>(this CommandBuilderContext<TArgument> commandBuilder, IScarletCommandManager commandManager)
        {
            ConcurrentCommandBase DecoratorFactory(ConcurrentCommandBase command)
            {
                return new SequentialAsyncCommandDecorator(commandManager, command);
            }

            commandBuilder.AddDecorator(DecoratorFactory);

            return commandBuilder;
        }
    }
}
