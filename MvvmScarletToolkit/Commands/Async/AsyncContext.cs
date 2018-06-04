using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit
{
    public class CommandContext<TKey> : CommandContext
    {
        public TKey Key { get; }

        public CommandContext(TKey key)
        {
            Key = key;
        }

        //TODO add subcontext management
        // - registration
        // - child subscriptions (verticals)
        // - subscriptions across children with the same parent (horizontals)
        // - totally unrelated subscriptions
    }

    public class CommandContext : ObservableObject, IBusy
    {
        private readonly ICollection<IExtendedAsyncCommand> _cache;
        private readonly List<IExtendedAsyncCommand> _stack;

        private bool _isBusy;
        public bool IsBusy
        {
            get { return _isBusy; }
            private set { SetValue(ref _isBusy, value); }
        }

        public CommandContext()
        {
            _cache = new HashSet<IExtendedAsyncCommand>();
            _stack = new List<IExtendedAsyncCommand>();
        }

        public void Register(IExtendedAsyncCommand command)
        {
            if (command == null)
                return;

            RegisterInternal(command);
        }

        private void RegisterInternal(IExtendedAsyncCommand command)
        {
            command.PropertyChanged += Command_PropertyChanged;
            _cache.Add(command);
        }

        private void Command_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(IExtendedAsyncCommand.IsBusy))
                return;

            if (!(sender is IExtendedAsyncCommand busyInstance))
                return;

            switch (busyInstance.IsBusy)
            {
                case true:
                    _stack.Add(busyInstance);
                    break;

                case false:
                    _stack.Remove(busyInstance);
                    break;
            }

            IsBusy = _stack.Count > 0;
        }

        public IExtendedAsyncCommand Create(Func<Task> execute)
        {
            var result = AsyncCommand.Create(execute);
            RegisterInternal(result);

            return result;
        }

        public IExtendedAsyncCommand Create(Func<Task> execute, Func<bool> canExecute)
        {
            var result = AsyncCommand.Create(execute, canExecute);
            RegisterInternal(result);

            return result;
        }

        public IExtendedAsyncCommand Create(Func<CancellationToken, Task> execute, Func<bool> canExecute)
        {
            var result = AsyncCommand.Create(execute, canExecute);
            RegisterInternal(result);

            return result;
        }

        public IExtendedAsyncCommand Create<TArgument>(Func<TArgument, CancellationToken, Task> execute, Func<TArgument, bool> canExecute)
        {
            var result = AsyncCommand.Create(execute, canExecute);
            RegisterInternal(result);

            return result;
        }

        public IExtendedAsyncCommand Create<TResult>(Func<CancellationToken, Task<TResult>> execute, Func<bool> canExecute)
        {
            var result = AsyncCommand.Create(execute, canExecute);
            RegisterInternal(result);

            return result;
        }

        public IExtendedAsyncCommand Create<TArgument, TResult>(Func<TArgument, CancellationToken, Task<TResult>> execute, Func<TArgument, bool> canExecute)
        {
            var result = AsyncCommand.Create(execute, canExecute);
            RegisterInternal(result);

            return result;
        }


    }
}
