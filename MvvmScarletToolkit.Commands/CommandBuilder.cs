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

        [Obsolete]
        public ICommandBuilderContext Create<TArgument, TResult>()
        {
            return Create<TArgument, TResult>(DefaultExecute<TArgument, TResult>, DefaultCanExecute);
        }

        public CommandBuilderContext<TArgument, TResult> Create<TArgument, TResult>(Func<TArgument, CancellationToken, Task<TResult>> execute)
        {
            return Create(execute, DefaultCanExecute);
        }

        [Obsolete]
        public CommandBuilderContext<TArgument, TResult> Create<TArgument, TResult>(Func<TArgument, bool> canExecute)
        {
            return Create(DefaultExecute<TArgument, TResult>, canExecute);
        }

        public CommandBuilderContext<TArgument, TResult> Create<TArgument, TResult>(Func<TArgument, CancellationToken, Task<TResult>> execute, Func<TArgument, bool> canExecute)
        {
            return new CommandBuilderContext<TArgument, TResult>(Dispatcher, CommandManager, _busyStackFactory, execute, canExecute);
        }

#pragma warning disable RCS1163 // Unused parameter.

        internal static Task<TResult> DefaultExecute<TArgument, TResult>(TArgument parameter, CancellationToken token)
#pragma warning restore RCS1163 // Unused parameter.
        {
            if (token.IsCancellationRequested)
            {
                throw new TaskCanceledException(); // not sure, maybe there is a better thing to do instead
            }

            return Task.FromResult<TResult>(default);
        }

#pragma warning disable RCS1163 // Unused parameter.

        internal static bool DefaultCanExecute<TArgument>(TArgument parameter)
#pragma warning restore RCS1163 // Unused parameter.
        {
            return true;
        }
    }
}
