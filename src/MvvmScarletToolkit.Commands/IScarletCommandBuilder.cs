using CommunityToolkit.Mvvm.Messaging;
using MvvmScarletToolkit.Commands;
using System.ComponentModel;

namespace MvvmScarletToolkit
{
    public interface IScarletCommandBuilder
    {
        IScarletExceptionHandler ExceptionHandler { get; }
        IScarletCommandManager CommandManager { get; }
        IScarletDispatcher Dispatcher { get; }
        IMessenger Messenger { get; }
        IExitService Exit { get; }
        IScarletEventManager<INotifyPropertyChanged, PropertyChangedEventArgs> WeakEventManager { get; }

        CommandBuilderContext<TArgument> Create<TArgument>(Func<TArgument, CancellationToken, Task> execute, Func<TArgument, bool> canExecute);
    }
}
