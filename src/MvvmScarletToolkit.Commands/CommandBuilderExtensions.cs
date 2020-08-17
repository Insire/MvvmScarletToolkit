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
            return builder.Create<object>((_, __) => execute(), (_) => true);
        }

        public static CommandBuilderContext<object> Create(this IScarletCommandBuilder builder, Func<Task> execute, Func<bool> canExecute)
        {
            return builder.Create<object>((_, __) => execute(), (_) => canExecute());
        }

        public static CommandBuilderContext<object> Create(this IScarletCommandBuilder builder, Func<CancellationToken, Task> execute)
        {
            return builder.Create<object>((_, token) => execute(token), (_) => true);
        }

        public static CommandBuilderContext<object> Create(this IScarletCommandBuilder builder, Func<CancellationToken, Task> execute, Func<bool> canExecute)
        {
            return builder.Create<object>((_, token) => execute(token), (_) => canExecute());
        }

        public static CommandBuilderContext<TArgument> Create<TArgument>(this IScarletCommandBuilder builder, Func<Task> execute)
        {
            return builder.Create<TArgument>((_, __) => execute(), (_) => true);
        }

        public static CommandBuilderContext<TArgument> Create<TArgument>(this IScarletCommandBuilder builder, Func<Task> execute, Func<bool> canExecute)
        {
            return builder.Create<TArgument>((_, __) => execute(), (_) => canExecute());
        }

        public static CommandBuilderContext<TArgument> Create<TArgument>(this IScarletCommandBuilder builder, Func<CancellationToken, Task> execute)
        {
            return builder.Create<TArgument>((_, token) => execute(token), (_) => true);
        }

        public static CommandBuilderContext<TArgument> Create<TArgument>(this IScarletCommandBuilder builder, Func<CancellationToken, Task> execute, Func<bool> canExecute)
        {
            return builder.Create<TArgument>((_, token) => execute(token), (_) => canExecute());
        }

        public static CommandBuilderContext<TArgument> Create<TArgument>(this IScarletCommandBuilder builder, Func<TArgument, Task> execute)
        {
            return builder.Create<TArgument>((parameter, _) => execute(parameter), (_) => true);
        }

        public static CommandBuilderContext<TArgument> Create<TArgument>(this IScarletCommandBuilder builder, Func<TArgument, Task> execute, Func<bool> canExecute)
        {
            return builder.Create<TArgument>((parameter, _) => execute(parameter), (_) => canExecute());
        }

        public static CommandBuilderContext<TArgument> Create<TArgument>(this IScarletCommandBuilder builder, Func<TArgument, Task> execute, Func<TArgument, bool> canExecute)
        {
            return builder.Create<TArgument>((parameter, _) => execute(parameter), (parameter) => canExecute(parameter));
        }

        public static CommandBuilderContext<TArgument> Create<TArgument>(this IScarletCommandBuilder builder, Func<TArgument, CancellationToken, Task> execute)
        {
            return builder.Create<TArgument>((parameter, token) => execute(parameter, token), (_) => true);
        }

        public static CommandBuilderContext<TArgument> Create<TArgument>(this IScarletCommandBuilder builder, Func<TArgument, CancellationToken, Task> execute, Func<bool> canExecute)
        {
            return builder.Create<TArgument>((parameter, token) => execute(parameter, token), (_) => canExecute());
        }

        public static CommandBuilderContext<TArgument> Create<TArgument>(this IScarletCommandBuilder builder, Func<TArgument, CancellationToken, Task> execute, Func<TArgument, bool> canExecute)
        {
            return builder.Create<TArgument>((parameter, token) => execute(parameter, token), (parameter) => canExecute(parameter));
        }
    }
}
