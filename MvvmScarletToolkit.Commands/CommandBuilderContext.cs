using MvvmScarletToolkit.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Commands
{
    public class CommandBuilderContext<TArgument, TResult> : AbstractBuilder<IExtendedAsyncCommand>, ICommandBuilderContext
    {
        private readonly Queue<Func<IExtendedAsyncCommand, IExtendedAsyncCommand>> _decorators;
        private readonly Func<TArgument, CancellationToken, Task<TResult>> _execute;
        private readonly Func<TArgument, bool> _canExcute;

        public IScarletDispatcher Dispatcher { get; }
        public IScarletCommandManager CommandManager { get; }
        public ICancelCommand CancelCommand { get; set; }

        // TODO add validation, providing notifications on why can execute returned false

        // TODO decide whether this should be reuseable

        private CommandBuilderContext(IScarletDispatcher dispatcher, IScarletCommandManager commandManager)
        {
            Dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
            CommandManager = commandManager ?? throw new ArgumentNullException(nameof(commandManager));

            CancelCommand = new CancelCommand(commandManager);

            _decorators = new Queue<Func<IExtendedAsyncCommand, IExtendedAsyncCommand>>();
        }

        public CommandBuilderContext(IScarletDispatcher dispatcher, IScarletCommandManager commandManager, Func<TArgument, CancellationToken, Task<TResult>> execute, Func<TArgument, bool> canExecute)
            : this(dispatcher, commandManager)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExcute = canExecute ?? throw new ArgumentNullException(nameof(canExecute));
        }

        public void AddDecorator(Func<IExtendedAsyncCommand, IExtendedAsyncCommand> decoratorFactory)
        {
            _decorators.Enqueue(decoratorFactory);
        }

        public override IExtendedAsyncCommand Build()
        {
            var result = default(ConcurrentCommand<TArgument, TResult>);
            if (_canExcute is null)
            {
                result = new ConcurrentCommand<TArgument, TResult>(CommandManager, CancelCommand, _execute);
            }
            else
            {
                result = new ConcurrentCommand<TArgument, TResult>(CommandManager, CancelCommand, _execute, _canExcute);
            }

            //TODO complete implementation

            return CreateDecoratorsOn(result);
        }

        private IExtendedAsyncCommand CreateDecoratorsOn(IExtendedAsyncCommand command)
        {
            var decorator = command;
            while (_decorators.Count > 0)
            {
                decorator = _decorators.Dequeue()(decorator);
            }

            return decorator;
        }
    }
}
