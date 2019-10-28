using MvvmScarletToolkit.Abstractions;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Commands
{
    public class CommandBuilder : ICommandBuilder
    {
        private readonly Func<Action<bool>, IBusyStack> _busyStackFactory;

        public IScarletDispatcher Dispatcher { get; }
        public IScarletCommandManager CommandManager { get; }
        public IScarletMessenger Messenger { get; }
        public IExitService Exit { get; }
        public IWeakEventManager<INotifyPropertyChanged, PropertyChangedEventArgs> WeakEventManager { get; }

        public CommandBuilder(IScarletDispatcher dispatcher, IScarletCommandManager commandManager, IScarletMessenger messenger, IExitService exitService, IWeakEventManager<INotifyPropertyChanged, PropertyChangedEventArgs> weakEventManager, Func<Action<bool>, IBusyStack> busyStackFactory)
        {
            Dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
            CommandManager = commandManager ?? throw new ArgumentNullException(nameof(commandManager));
            Exit = exitService ?? throw new ArgumentNullException(nameof(exitService));
            Messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
            WeakEventManager = weakEventManager ?? throw new ArgumentNullException(nameof(weakEventManager));

            _busyStackFactory = busyStackFactory ?? throw new ArgumentNullException(nameof(busyStackFactory));
        }

        public CommandBuilderContext<TArgument, TResult> Create<TArgument, TResult>(Func<TArgument, CancellationToken, Task<TResult>> execute, Func<TArgument, bool> canExecute)
        {
            return new CommandBuilderContext<TArgument, TResult>(Dispatcher, CommandManager, _busyStackFactory, execute, canExecute);
        }
    }
}
