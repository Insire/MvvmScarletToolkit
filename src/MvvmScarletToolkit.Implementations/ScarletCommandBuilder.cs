using MvvmScarletToolkit.Abstractions;
using MvvmScarletToolkit.Commands;
using MvvmScarletToolkit.Observables;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit
{
    public class ScarletCommandBuilder : ICommandBuilder
    {
        private static readonly Lazy<ScarletCommandBuilder> _default = new Lazy<ScarletCommandBuilder>(() => new ScarletCommandBuilder(ScarletDispatcher.Default, ScarletCommandManager.Default, ScarletMessenger.Default, ScarletExitService.Default, ScarletWeakEventManager.Default, (lambda) => new BusyStack(lambda, ScarletDispatcher.Default)));

        public static ICommandBuilder Default => _default.Value;

        private readonly Func<Action<bool>, IBusyStack> _busyStackFactory;

        public IScarletDispatcher Dispatcher { get; }
        public IScarletCommandManager CommandManager { get; }
        public IScarletMessenger Messenger { get; }
        public IExitService Exit { get; }
        public IScarletEventManager<INotifyPropertyChanged, PropertyChangedEventArgs> WeakEventManager { get; }

        public ScarletCommandBuilder(IScarletDispatcher dispatcher, IScarletCommandManager commandManager, IScarletMessenger messenger, IExitService exitService, IScarletEventManager<INotifyPropertyChanged, PropertyChangedEventArgs> weakEventManager, Func<Action<bool>, IBusyStack> busyStackFactory)
        {
            Dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
            CommandManager = commandManager ?? throw new ArgumentNullException(nameof(commandManager));
            Exit = exitService ?? throw new ArgumentNullException(nameof(exitService));
            Messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
            WeakEventManager = weakEventManager ?? throw new ArgumentNullException(nameof(weakEventManager));

            _busyStackFactory = busyStackFactory ?? throw new ArgumentNullException(nameof(busyStackFactory));
        }

        public CommandBuilderContext<TArgument> Create<TArgument>(Func<TArgument, CancellationToken, Task> execute, Func<TArgument, bool> canExecute)
        {
            return new CommandBuilderContext<TArgument>(Dispatcher, CommandManager, _busyStackFactory, execute, canExecute);
        }
    }
}
