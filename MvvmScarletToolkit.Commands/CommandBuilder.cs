using MvvmScarletToolkit.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Commands
{
    public class CommandBuilder : ICommandBuilder
    {
        private readonly Func<Action<bool>, IBusyStack> _busyStackFactory;

        public IScarletDispatcher Dispatcher { get; }
        public IScarletCommandManager CommandManager { get; }

        public CommandBuilder(IScarletDispatcher dispatcher, IScarletCommandManager commandManager, Func<Action<bool>, IBusyStack> busyStackFactory)
        {
            Dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
            CommandManager = commandManager ?? throw new ArgumentNullException(nameof(commandManager));

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

        internal static Task<TResult> DefaultExecute<TArgument, TResult>(TArgument parameter, CancellationToken token)
        {
            if (token.IsCancellationRequested)
            {
                throw new TaskCanceledException(); // not sure, maybe there is a better thing to do instead
            }

            return Task.FromResult<TResult>(default);
        }

        internal static bool DefaultCanExecute<TArgument>(TArgument parameter)
        {
            return true;
        }
    }
}
