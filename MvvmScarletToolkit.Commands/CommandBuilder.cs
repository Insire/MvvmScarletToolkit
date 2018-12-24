using MvvmScarletToolkit.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Commands
{
    public class CommandBuilder<TArgument, TResult>
    {
        public Func<TArgument, CancellationToken, Task<TResult>> Execute { get; set; }
        public Func<TArgument, bool> CanExcute { get; set; }

        public Func<IExtendedAsyncCommand, IExtendedAsyncCommand> Decorators { get; set; }
        public Func<IExtendedAsyncCommand> Use { get; set; }

        public IScarletDispatcher Dispatcher { get; set; }
        public ICommandManager CommandManager { get; set; }

        private CommandBuilder()
        {
            Execute = DefaultExecute;
            CanExcute = DefaultCanExecute;
            Use = DefaultUse;
        }

        public CommandBuilder(IScarletDispatcher dispatcher, ICommandManager commandManager)
            : this()
        {
            Dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
            CommandManager = commandManager ?? throw new ArgumentNullException(nameof(commandManager));
        }

        private IExtendedAsyncCommand DefaultUse()
        {
            return new ConcurrentAsyncCommand<TArgument, TResult>(CommandManager, Execute, CanExcute);
        }

        private static Task<TResult> DefaultExecute(TArgument parameter, CancellationToken token)
        {
            if (token.IsCancellationRequested)
            {
                throw new TaskCanceledException();
            }

            return default;
        }

        private static bool DefaultCanExecute(TArgument parameter)
        {
            return true;
        }

        // register decorator
        // provide custom cancellation action
        // optionally enable async cancellation?
        // validation, providing notifications on why can execute returned false
    }
}
