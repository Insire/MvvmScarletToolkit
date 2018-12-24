using MvvmScarletToolkit.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Commands
{
    public static class AsyncCommand
    {
        public static IExtendedAsyncCommand Create(Func<Task> command, ICommandManager commandManager)
        {
            return new ConcurrentAsyncCommand<object, object>(commandManager, async (_, __) =>
             {
                 await command().ConfigureAwait(false);
                 return default;
             });
        }

        public static IExtendedAsyncCommand Create(Func<Task> command, Func<bool> canExecute, ICommandManager commandManager)
        {
            return new ConcurrentAsyncCommand<object, object>(commandManager, async (_, __) =>
             {
                 await command().ConfigureAwait(false);
                 return default;
             }, _ => canExecute());
        }

        public static IExtendedAsyncCommand Create(Func<object, Task> command, Func<bool> canExecute, ICommandManager commandManager)
        {
            return new ConcurrentAsyncCommand<object, object>(commandManager, async (_, token) =>
             {
                 await command(token).ConfigureAwait(false);
                 return default;
             }, _ => canExecute());
        }

        public static IExtendedAsyncCommand Create(Func<CancellationToken, Task> command, Func<bool> canExecute, ICommandManager commandManager)
        {
            return new ConcurrentAsyncCommand<object, object>(commandManager, async (_, token) =>
             {
                 await command(token).ConfigureAwait(false);
                 return default;
             }, _ => canExecute());
        }

        public static IExtendedAsyncCommand Create<TArgument>(Func<TArgument, CancellationToken, Task> command, Func<TArgument, bool> canExecute, ICommandManager commandManager)
        {
            return new ConcurrentAsyncCommand<TArgument, object>(commandManager, async (arg, token) =>
             {
                 await command(arg, token).ConfigureAwait(false);
                 return default;
             }, (arg) => canExecute(arg));
        }

        public static IExtendedAsyncCommand Create<TArgument>(Func<TArgument, Task> command, Func<TArgument, bool> canExecute, ICommandManager commandManager)
        {
            return new ConcurrentAsyncCommand<TArgument, object>(commandManager, async (arg, _) =>
             {
                 await command(arg).ConfigureAwait(false);
                 return default;
             }, (arg) => canExecute(arg));
        }

        public static IExtendedAsyncCommand Create<TResult>(Func<Task<TResult>> command, Func<bool> canExecute, ICommandManager commandManager)
        {
            return new ConcurrentAsyncCommand<object, TResult>(commandManager, async (_, __) => await command().ConfigureAwait(false), _ => canExecute());
        }

        public static IExtendedAsyncCommand Create<TResult>(Func<CancellationToken, Task<TResult>> command, Func<bool> canExecute, ICommandManager commandManager)
        {
            return new ConcurrentAsyncCommand<object, TResult>(commandManager, async (_, token) => await command(token).ConfigureAwait(false), _ => canExecute());
        }

        public static IExtendedAsyncCommand Create<TArgument, TResult>(Func<TArgument, CancellationToken, Task<TResult>> command, Func<TArgument, bool> canExecute, ICommandManager commandManager)
        {
            return new ConcurrentAsyncCommand<TArgument, TResult>(commandManager, command, arg => canExecute(arg));
        }

        /// <summary>
        ///Configure a command as to not automatically cancel and rerun when executed again
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public static IExtendedAsyncCommand AsSequential(this IExtendedAsyncCommand command)
        {
            return new SequentialAsyncCommandDecorator(command);
        }
    }
}
