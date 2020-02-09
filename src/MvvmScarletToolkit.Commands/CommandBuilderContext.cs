using MvvmScarletToolkit.Abstractions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Commands
{
    /// <summary>
    /// Context class for creating and configuring <see cref="MvvmScarletToolkit.Commands.ConcurrentCommand{TArgument}"/> via the fluent builder pattern
    /// </summary>
    /// <typeparam name="TArgument">The argument type, that the to be created command is supposed to accept</typeparam>
    [Bindable(false)]
    public sealed class CommandBuilderContext<TArgument> : AbstractBuilder<ConcurrentCommandBase>
    {
        private readonly Queue<Func<ConcurrentCommandBase, ConcurrentCommandBase>> _decorators;
        private readonly Func<Action<bool>, IBusyStack> _busyStackFactory;

        private readonly Func<TArgument, CancellationToken, Task>? _execute;
        private readonly Func<TArgument, bool>? _canExcute;

        internal IScarletCommandManager CommandManager { get; }

        /// <summary>
        /// optional <see cref="System.Windows.Input.ICommand"/> for Cancellation-Support
        /// </summary>
        internal ICancelCommand? CancelCommand { get; set; }

        /// <summary>
        /// optional external <see cref="IBusyStack"/> that can be hooked up to receive notifications from the internal busy state
        /// </summary>
        internal IBusyStack? BusyStack { get; set; }

        public CommandBuilderContext(IScarletCommandManager commandManager, Func<Action<bool>, IBusyStack> busyStackFactory, Func<TArgument, CancellationToken, Task> execute, Func<TArgument, bool> canExecute)
        {
            CommandManager = commandManager ?? throw new ArgumentNullException(nameof(commandManager));

            _decorators = new Queue<Func<ConcurrentCommandBase, ConcurrentCommandBase>>();
            _busyStackFactory = busyStackFactory ?? throw new ArgumentNullException(nameof(busyStackFactory));

            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExcute = canExecute ?? throw new ArgumentNullException(nameof(canExecute));
        }

        internal void AddDecorator(Func<ConcurrentCommandBase, ConcurrentCommandBase> decoratorFactory)
        {
            if (decoratorFactory is null)
            {
                throw new ArgumentNullException(nameof(decoratorFactory) + " can't be null.");
            }

            _decorators.Enqueue(decoratorFactory);
        }

        public override ConcurrentCommandBase Build()
        {
            var command = CreateCommand();

            return WrapInDecorators(command);
        }

        private ConcurrentCommand<TArgument> CreateCommand()
        {
            if (_canExcute is null)
            {
                return new ConcurrentCommand<TArgument>(CommandManager, CancelCommand ?? new CancelCommand(CommandManager), _busyStackFactory, _execute);
            }

            if (BusyStack is null)
            {
                return new ConcurrentCommand<TArgument>(CommandManager, CancelCommand ?? new CancelCommand(CommandManager), _busyStackFactory, _execute, _canExcute);
            }

            return new ConcurrentCommand<TArgument>(CommandManager, CancelCommand ?? new CancelCommand(CommandManager), _busyStackFactory, BusyStack, _execute, _canExcute);
        }

        private ConcurrentCommandBase WrapInDecorators(ConcurrentCommandBase command)
        {
            var decorator = command;
            while (_decorators.Count > 0)
            {
                var wrapper = _decorators.Dequeue();
                decorator = wrapper.Invoke(decorator);
            }

            return decorator;
        }
    }
}
