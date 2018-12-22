using MvvmScarletToolkit.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Commands
{
    public static class AsyncCommand
    {
        public static IExtendedAsyncCommand Create(Func<Task> command)
        {
            return new ConcurrentAsyncCommand<object, object>(async (_, __) =>
            {
                await command().ConfigureAwait(false);
                return null;
            });
        }

        public static IExtendedAsyncCommand Create(Func<Task> command, Func<bool> canExecute)
        {
            return new ConcurrentAsyncCommand<object, object>(async (_, __) =>
            {
                await command().ConfigureAwait(false);
                return null;
            }, _ => canExecute());
        }

        public static IExtendedAsyncCommand Create(Func<object, Task> command, Func<bool> canExecute)
        {
            return new ConcurrentAsyncCommand<object, object>(async (_, token) =>
            {
                await command(token).ConfigureAwait(false);
                return null;
            }, _ => canExecute());
        }

        public static IExtendedAsyncCommand Create(Func<CancellationToken, Task> command, Func<bool> canExecute)
        {
            return new ConcurrentAsyncCommand<object, object>(async (_, token) =>
            {
                await command(token).ConfigureAwait(false);
                return null;
            }, _ => canExecute());
        }

        public static IExtendedAsyncCommand Create<TArgument>(Func<TArgument, CancellationToken, Task> command, Func<TArgument, bool> canExecute)
        {
            return new ConcurrentAsyncCommand<TArgument, object>(async (arg, token) =>
            {
                await command(arg, token).ConfigureAwait(false);
                return null;
            }, (arg) => canExecute(arg));
        }

        public static IExtendedAsyncCommand Create<TArgument>(Func<TArgument, Task> command, Func<TArgument, bool> canExecute)
        {
            return new ConcurrentAsyncCommand<TArgument, object>(async (arg, _) =>
            {
                await command(arg).ConfigureAwait(false);
                return null;
            }, (arg) => canExecute(arg));
        }

        public static IExtendedAsyncCommand Create<TResult>(Func<Task<TResult>> command, Func<bool> canExecute)
        {
            return new ConcurrentAsyncCommand<object, TResult>(async (_, __) => await command().ConfigureAwait(false), _ => canExecute());
        }

        public static IExtendedAsyncCommand Create<TResult>(Func<CancellationToken, Task<TResult>> command, Func<bool> canExecute)
        {
            return new ConcurrentAsyncCommand<object, TResult>(async (_, token) => await command(token).ConfigureAwait(false), _ => canExecute());
        }

        public static IExtendedAsyncCommand Create<TArgument, TResult>(Func<TArgument, CancellationToken, Task<TResult>> command, Func<TArgument, bool> canExecute)
        {
            return new ConcurrentAsyncCommand<TArgument, TResult>(command, arg => canExecute(arg));
        }

        public static IExtendedAsyncCommand AsSequential(this IExtendedAsyncCommand command)
        {
            return new SequentialAsyncCommandDecorator(command);
        }
    }
}
