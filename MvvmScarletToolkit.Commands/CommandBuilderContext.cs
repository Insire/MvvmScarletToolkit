using MvvmScarletToolkit.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Commands
{
    // TODO add validation, providing notifications on why can execute returned false
    [System.Diagnostics.CodeAnalysis.SuppressMessage("PropertyChangedAnalyzers.PropertyChanged", "INPC001:The class has mutable properties and should implement INotifyPropertyChanged.", Justification = "Class is not meant to be bound.")]
    public class CommandBuilderContext<TArgument, TResult> : AbstractBuilder<ConcurrentCommandBase>
    {
        private readonly Queue<Func<ConcurrentCommandBase, ConcurrentCommandBase>> _decorators;
        private readonly Func<TArgument, CancellationToken, Task<TResult>> _execute;
        private readonly Func<TArgument, bool> _canExcute;
        private readonly Func<Action<bool>, IBusyStack> _busyStackFactory;

        public IScarletDispatcher Dispatcher { get; }

        public IScarletCommandManager CommandManager { get; }

        /// <summary>
        /// can be configured externally
        /// </summary>
        public ICancelCommand CancelCommand { get; set; }

        /// <summary>
        /// can be configured externally
        /// </summary>
        public IBusyStack BusyStack { get; set; }

        private CommandBuilderContext(IScarletDispatcher dispatcher, IScarletCommandManager commandManager, Func<Action<bool>, IBusyStack> busyStackFactory)
        {
            Dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
            CommandManager = commandManager ?? throw new ArgumentNullException(nameof(commandManager));

            CancelCommand = new CancelCommand(commandManager);

            _decorators = new Queue<Func<ConcurrentCommandBase, ConcurrentCommandBase>>();
            _busyStackFactory = busyStackFactory ?? throw new ArgumentNullException(nameof(busyStackFactory));
        }

        public CommandBuilderContext(IScarletDispatcher dispatcher, IScarletCommandManager commandManager, Func<Action<bool>, IBusyStack> busyStackFactory, Func<TArgument, CancellationToken, Task<TResult>> execute, Func<TArgument, bool> canExecute)
            : this(dispatcher, commandManager, busyStackFactory)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExcute = canExecute ?? throw new ArgumentNullException(nameof(canExecute));
        }

        public void AddDecorator(Func<ConcurrentCommandBase, ConcurrentCommandBase> decoratorFactory)
        {
            _decorators.Enqueue(decoratorFactory);
        }

        public override ConcurrentCommandBase Build()
        {
            var result = default(ConcurrentCommand<TArgument, TResult>);
            if (_canExcute is null)
            {
                result = new ConcurrentCommand<TArgument, TResult>(CommandManager, CancelCommand, _busyStackFactory, _execute);
            }
            else
            {
                if (BusyStack is null)
                {
                    result = new ConcurrentCommand<TArgument, TResult>(CommandManager, CancelCommand, _busyStackFactory, _execute, _canExcute);
                }
                else
                {
                    result = new ConcurrentCommand<TArgument, TResult>(CommandManager, CancelCommand, _busyStackFactory, BusyStack, _execute, _canExcute);
                }
            }

            //TODO complete implementation

            return CreateDecoratorsOn(result);

            ConcurrentCommandBase CreateDecoratorsOn(ConcurrentCommandBase command)
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
}
