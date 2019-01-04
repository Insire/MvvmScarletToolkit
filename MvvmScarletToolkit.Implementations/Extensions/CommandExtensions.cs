using MvvmScarletToolkit.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Implementations
{
    internal static class AsyncCommand
    {
        public static IExtendedAsyncCommand Create(Func<Task> command)
        {
            return MvvmScarletToolkit.Commands.AsyncCommand.Create(command, ScarletCommandManager.Default);
        }

        public static IExtendedAsyncCommand Create(Func<Task> command, Func<bool> canExecute)
        {
            return MvvmScarletToolkit.Commands.AsyncCommand.Create(command, canExecute, ScarletCommandManager.Default);
        }

        public static IExtendedAsyncCommand Create(Func<object, Task> command, Func<bool> canExecute)
        {
            return MvvmScarletToolkit.Commands.AsyncCommand.Create(command, canExecute, ScarletCommandManager.Default);
        }

        public static IExtendedAsyncCommand Create(Func<CancellationToken, Task> command, Func<bool> canExecute)
        {
            return MvvmScarletToolkit.Commands.AsyncCommand.Create(command, canExecute, ScarletCommandManager.Default);
        }

        public static IExtendedAsyncCommand Create<TArgument>(Func<TArgument, CancellationToken, Task> command, Func<TArgument, bool> canExecute)
        {
            return MvvmScarletToolkit.Commands.AsyncCommand.Create(command, canExecute, ScarletCommandManager.Default);
        }

        public static IExtendedAsyncCommand Create<TArgument>(Func<TArgument, Task> command, Func<TArgument, bool> canExecute)
        {
            return MvvmScarletToolkit.Commands.AsyncCommand.Create(command, canExecute, ScarletCommandManager.Default);
        }

        public static IExtendedAsyncCommand Create<TResult>(Func<Task<TResult>> command, Func<bool> canExecute)
        {
            return MvvmScarletToolkit.Commands.AsyncCommand.Create(command, canExecute, ScarletCommandManager.Default);
        }

        public static IExtendedAsyncCommand Create<TResult>(Func<CancellationToken, Task<TResult>> command, Func<bool> canExecute)
        {
            return MvvmScarletToolkit.Commands.AsyncCommand.Create(command, canExecute, ScarletCommandManager.Default);
        }

        public static IExtendedAsyncCommand Create<TArgument, TResult>(Func<TArgument, CancellationToken, Task<TResult>> command, Func<TArgument, bool> canExecute)
        {
            return MvvmScarletToolkit.Commands.AsyncCommand.Create(command, canExecute, ScarletCommandManager.Default);
        }
    }
}
