using MvvmScarletToolkit.Commands;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Abstractions
{
    public interface ICommandBuilder
    {
        IScarletCommandManager CommandManager { get; }
        IScarletDispatcher Dispatcher { get; }

        ICommandBuilderContext Create<TArgument, TResult>();

        CommandBuilderContext<TArgument, TResult> Create<TArgument, TResult>(Func<TArgument, bool> canExecute);

        CommandBuilderContext<TArgument, TResult> Create<TArgument, TResult>(Func<TArgument, CancellationToken, Task<TResult>> execute);

        CommandBuilderContext<TArgument, TResult> Create<TArgument, TResult>(Func<TArgument, CancellationToken, Task<TResult>> execute, Func<TArgument, bool> canExecute);
    }
}
