using MvvmScarletToolkit.Commands;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit
{
    public static class CommandBuilderExtensions
    {
        public static CommandBuilderContext<object> Create(this IScarletCommandBuilder builder, Func<Task> execute)
        {
            return builder.Create<object>((parameter, token) => execute(), (parameter) => true);
        }

        public static CommandBuilderContext<object> Create(this IScarletCommandBuilder builder, Func<Task> execute, Func<bool> canExecute)
        {
            return builder.Create<object>((parameter, token) => execute(), (parameter) => canExecute());
        }

        public static CommandBuilderContext<object> Create(this IScarletCommandBuilder builder, Func<CancellationToken, Task> execute)
        {
            return builder.Create<object>((parameter, token) => execute(token), (parameter) => true);
        }

        public static CommandBuilderContext<object> Create(this IScarletCommandBuilder builder, Func<CancellationToken, Task> execute, Func<bool> canExecute)
        {
            return builder.Create<object>((parameter, token) => execute(token), (parameter) => canExecute());
        }

        public static CommandBuilderContext<TArgument> Create<TArgument>(this IScarletCommandBuilder builder, Func<Task> execute)
        {
            return builder.Create<TArgument>((parameter, token) => execute(), (parameter) => true);
        }

        public static CommandBuilderContext<TArgument> Create<TArgument>(this IScarletCommandBuilder builder, Func<Task> execute, Func<bool> canExecute)
        {
            return builder.Create<TArgument>((parameter, token) => execute(), (parameter) => canExecute());
        }

        public static CommandBuilderContext<TArgument> Create<TArgument>(this IScarletCommandBuilder builder, Func<CancellationToken, Task> execute)
        {
            return builder.Create<TArgument>((parameter, token) => execute(token), (parameter) => true);
        }

        public static CommandBuilderContext<TArgument> Create<TArgument>(this IScarletCommandBuilder builder, Func<CancellationToken, Task> execute, Func<bool> canExecute)
        {
            return builder.Create<TArgument>((parameter, token) => execute(token), (parameter) => canExecute());
        }

        public static CommandBuilderContext<TArgument> Create<TArgument>(this IScarletCommandBuilder builder, Func<TArgument, Task> execute)
        {
            return builder.Create<TArgument>((parameter, token) => execute(parameter), (parameter) => true);
        }

        public static CommandBuilderContext<TArgument> Create<TArgument>(this IScarletCommandBuilder builder, Func<TArgument, Task> execute, Func<bool> canExecute)
        {
            return builder.Create<TArgument>((parameter, token) => execute(parameter), (parameter) => canExecute());
        }

        public static CommandBuilderContext<TArgument> Create<TArgument>(this IScarletCommandBuilder builder, Func<TArgument, Task> execute, Func<TArgument, bool> canExecute)
        {
            return builder.Create<TArgument>((parameter, token) => execute(parameter), (parameter) => canExecute(parameter));
        }

        public static CommandBuilderContext<TArgument> Create<TArgument>(this IScarletCommandBuilder builder, Func<TArgument, CancellationToken, Task> execute)
        {
            return builder.Create<TArgument>((parameter, token) => execute(parameter, token), (parameter) => true);
        }

        public static CommandBuilderContext<TArgument> Create<TArgument>(this IScarletCommandBuilder builder, Func<TArgument, CancellationToken, Task> execute, Func<bool> canExecute)
        {
            return builder.Create<TArgument>((parameter, token) => execute(parameter, token), (parameter) => canExecute());
        }

        public static CommandBuilderContext<TArgument> Create<TArgument>(this IScarletCommandBuilder builder, Func<TArgument, CancellationToken, Task> execute, Func<TArgument, bool> canExecute)
        {
            return builder.Create<TArgument>((parameter, token) => execute(parameter, token), (parameter) => canExecute(parameter));
        }
    }
}
