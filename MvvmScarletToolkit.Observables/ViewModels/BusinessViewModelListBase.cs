using MvvmScarletToolkit.Abstractions;
using MvvmScarletToolkit.Commands;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MvvmScarletToolkit.Observables
{
    /// <summary>
    /// Collection ViewModelBase that bootstraps loading, unloading and refreshing of its content
    /// </summary>
    /// <typeparam name="TViewModel"></typeparam>
    public abstract class BusinessViewModelListBase<TViewModel> : ViewModelListBase<TViewModel>, IBusinessViewModelListBase
        where TViewModel : class, INotifyPropertyChanged
    {
        private bool _isLoaded;
        [Bindable(true, BindingDirection.OneWay)]
        public bool IsLoaded
        {
            get { return _isLoaded; }
            protected set { SetValue(ref _isLoaded, value); }
        }

        [Bindable(true, BindingDirection.OneWay)]
        public virtual ICommand RemoveRangeCommand { get; }

        [Bindable(true, BindingDirection.OneWay)]
        public virtual ICommand RemoveCommand { get; }

        [Bindable(true, BindingDirection.OneWay)]
        public virtual ICommand ClearCommand { get; }

        private readonly ConcurrentCommandBase _loadCommand;
        [Bindable(true, BindingDirection.OneWay)]
        public virtual ICommand LoadCommand => _loadCommand;

        private readonly ConcurrentCommandBase _refreshCommand;
        [Bindable(true, BindingDirection.OneWay)]
        public virtual ICommand RefreshCommand => _refreshCommand;

        private readonly ConcurrentCommandBase _unloadCommand;
        [Bindable(true, BindingDirection.OneWay)]
        public virtual ICommand UnloadCommand => _unloadCommand;

        protected BusinessViewModelListBase(ICommandBuilder commandBuilder)
            : base(commandBuilder)
        {
            RemoveCommand = commandBuilder
                .Create(Remove, CanRemove)
                .WithSingleExecution(CommandManager)
                .WithBusyNotification(BusyStack)
                .WithCancellation()
                .Build();

            RemoveRangeCommand = commandBuilder
                .Create(RemoveRange, CanRemoveRange)
                .WithSingleExecution(CommandManager)
                .WithBusyNotification(BusyStack)
                .WithCancellation()
                .Build();

            ClearCommand = commandBuilder
                .Create(Clear, CanClear)
                .WithSingleExecution(CommandManager)
                .WithBusyNotification(BusyStack)
                .WithCancellation()
                .Build();

            _loadCommand = commandBuilder
                .Create(Load, CanLoad)
                .WithSingleExecution(CommandManager)
                .WithBusyNotification(BusyStack)
                .WithCancellation()
                .Build();

            _refreshCommand = commandBuilder
                .Create(Refresh, CanRefresh)
                .WithSingleExecution(CommandManager)
                .WithBusyNotification(BusyStack)
                .WithCancellation()
                .Build();

            _unloadCommand = commandBuilder
                .Create(Unload, CanUnload)
                .WithSingleExecution(CommandManager)
                .WithBusyNotification(BusyStack)
                .Build();

            Exit.UnloadOnExit(this);
        }

        protected virtual Task LoadInternal(CancellationToken token)
        {
            return RefreshInternal(token);
        }

        public async Task Load(CancellationToken token)
        {
            if (IsLoaded)
            {
                return;
            }

            using (BusyStack.GetToken())
            {
                await LoadInternal(token).ConfigureAwait(false);
                await Dispatcher.Invoke(() => IsLoaded = true).ConfigureAwait(false);
            }
        }

        protected virtual bool CanLoad()
        {
            return !IsLoaded
                && !_loadCommand.IsBusy
                && !_refreshCommand.IsBusy
                && !_unloadCommand.IsBusy;
        }

        protected virtual async Task UnloadInternal(CancellationToken token)
        {
            using (BusyStack.GetToken())
            {
                await Clear(token).ConfigureAwait(false);
            }
        }

        public async Task Unload(CancellationToken token)
        {
            using (BusyStack.GetToken())
            {
                await UnloadInternal(token).ConfigureAwait(false);
                await Dispatcher.Invoke(() => IsLoaded = false).ConfigureAwait(false);
            }
        }

        protected virtual bool CanUnload()
        {
            return !IsLoaded
                && !_loadCommand.IsBusy
                && !_refreshCommand.IsBusy
                && !_unloadCommand.IsBusy;
        }

        protected abstract Task RefreshInternal(CancellationToken token);

        public async Task Refresh(CancellationToken token)
        {
            using (BusyStack.GetToken())
            {
                await Clear(token).ConfigureAwait(false);
                await RefreshInternal(token).ConfigureAwait(false);
            }
        }

        protected virtual bool CanRefresh()
        {
            return IsLoaded
                && !_loadCommand.IsBusy
                && !_refreshCommand.IsBusy
                && !_unloadCommand.IsBusy;
        }

        private async Task RemoveRange(IList items)
        {
            using (BusyStack.GetToken())
            {
                await RemoveRange(items?.Cast<TViewModel>()).ConfigureAwait(false);
            }
        }

        protected virtual bool CanRemove(TViewModel item)
        {
            return CanClear()
                && !(item is null)
                && _items.Contains(item);
        }

        protected virtual bool CanRemoveRange(IEnumerable<TViewModel> items)
        {
            return CanClear()
                && items?.Any(p => _items.Contains(p)) == true;
        }

        protected bool CanRemoveRange(IList items)
        {
            return CanRemoveRange(items?.Cast<TViewModel>());
        }

        private bool CanRemoveRange()
        {
            return CanRemoveRange(SelectedItems);
        }

        private bool CanRemove()
        {
            return CanRemove(SelectedItem);
        }

        private Task RemoveRange()
        {
            return RemoveRange(SelectedItems);
        }
    }
}
