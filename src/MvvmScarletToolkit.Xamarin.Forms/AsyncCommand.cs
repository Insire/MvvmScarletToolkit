using MvvmScarletToolkit.Commands;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit
{
    public static class AsyncCommand
    {
        public static IAsyncCommand Create(Func<Task> command)
        {
            return new AsyncCommand<object>(ScarletCommandManager.Default, (parameter, token) => command());
        }

        public static IAsyncCommand Create(Func<Task> command, Func<bool> canExecute)
        {
            return new AsyncCommand<object>(ScarletCommandManager.Default, (parameter, token) => command(), (parameter) => canExecute());
        }

        public static IAsyncCommand Create<TArgument>(Func<Task<TArgument>> command, Func<bool> canExecute)
        {
            return new AsyncCommand<TArgument>(ScarletCommandManager.Default, (parameter, token) => command(), (parameter) => canExecute());
        }

        public static IAsyncCommand Create(Func<CancellationToken, Task> command, Func<bool> canExecute)
        {
            return new AsyncCommand<object>(ScarletCommandManager.Default, (parameter, token) => command(token), (parameter) => canExecute());
        }

        public static IAsyncCommand Create<TArgument>(Func<CancellationToken, Task> command, Func<bool> canExecute)
        {
            return new AsyncCommand<TArgument>(ScarletCommandManager.Default, (parameter, token) => command(token), (parameter) => canExecute());
        }

        public static IAsyncCommand Create<TArgument>(Func<TArgument, CancellationToken, Task> command, Func<TArgument, bool> canExecute)
        {
            return new AsyncCommand<TArgument>(ScarletCommandManager.Default, (parameter, token) => command(parameter, token), (parameter) => canExecute(parameter));
        }
    }
}
