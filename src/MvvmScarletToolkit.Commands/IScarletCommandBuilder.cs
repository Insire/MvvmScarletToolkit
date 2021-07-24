using Microsoft.Toolkit.Mvvm.Messaging;
using MvvmScarletToolkit.Commands;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit
{
    public interface IScarletCommandBuilder
    {
        IScarletCommandManager CommandManager { get; }
        IScarletDispatcher Dispatcher { get; }
        IMessenger Messenger { get; }
        IExitService Exit { get; }
        IScarletEventManager<INotifyPropertyChanged, PropertyChangedEventArgs> WeakEventManager { get; }

        CommandBuilderContext<TArgument> Create<TArgument>(Func<TArgument, CancellationToken, Task> execute, Func<TArgument, bool> canExecute);
    }
}
