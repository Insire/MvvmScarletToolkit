using MvvmScarletToolkit.Abstractions;
using MvvmScarletToolkit.Commands;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit
{
    public interface ICommandBuilder
    {
        IScarletCommandManager CommandManager { get; }
        IScarletDispatcher Dispatcher { get; }
        IScarletMessenger Messenger { get; }
        IExitService Exit { get; }
        IWeakEventManager<INotifyPropertyChanged, PropertyChangedEventArgs> WeakEventManager { get; }

        CommandBuilderContext<TArgument, TResult> Create<TArgument, TResult>(Func<TArgument, CancellationToken, Task<TResult>> execute, Func<TArgument, bool> canExecute);
    }
}
