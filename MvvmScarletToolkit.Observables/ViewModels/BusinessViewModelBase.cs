using MvvmScarletToolkit.Abstractions;
using MvvmScarletToolkit.Commands;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MvvmScarletToolkit.Observables
{
    /// <summary>
    /// ViewModelBase that bootstraps loading, unloading and refreshing of its content
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    public abstract class BusinessViewModelBase<TModel> : BusinessViewModelBase
        where TModel : class
    {
        private TModel _model;
        [Bindable(true, BindingDirection.OneWay)]
        public TModel Model
        {
            get { return _model; }
            protected set { SetValue(ref _model, value); }
        }

        protected BusinessViewModelBase(ICommandBuilder commandBuilder)
            : base(commandBuilder)
        {
        }

        protected BusinessViewModelBase(ICommandBuilder commandBuilder, TModel model)
            : base(commandBuilder)
        {
            Model = model ?? throw new ArgumentNullException(nameof(model));
        }
    }

    /// <summary>
    /// ViewModelBase that bootstraps loading, unloading and refreshing of its content
    /// </summary>
    public abstract class BusinessViewModelBase : ViewModelBase, IBusinessViewModelBase
    {
        private bool _isLoaded;
        [Bindable(true, BindingDirection.OneWay)]
        public bool IsLoaded
        {
            get { return _isLoaded; }
            protected set { SetValue(ref _isLoaded, value); }
        }

        private readonly ConcurrentCommandBase _loadCommand;
        [Bindable(true, BindingDirection.OneWay)]
        public virtual ICommand LoadCommand => _loadCommand;

        private readonly ConcurrentCommandBase _refreshCommand;
        [Bindable(true, BindingDirection.OneWay)]
        public virtual ICommand RefreshCommand => _refreshCommand;

        private readonly ConcurrentCommandBase _unloadCommand;
        [Bindable(true, BindingDirection.OneWay)]
        public virtual ICommand UnloadCommand => _unloadCommand;

        protected BusinessViewModelBase(ICommandBuilder commandBuilder)
            : base(commandBuilder)
        {
            _loadCommand = commandBuilder
                .Create(Load, CanLoad)
                .WithSingleExecution(CommandManager)
                .WithBusyNotification(BusyStack)
                .WithAsyncCancellation()
                .Build();

            _refreshCommand = commandBuilder
                .Create(Refresh, CanRefresh)
                .WithSingleExecution(CommandManager)
                .WithBusyNotification(BusyStack)
                .WithAsyncCancellation()
                .Build();

            _unloadCommand = commandBuilder
                .Create(Unload, CanUnload)
                .WithSingleExecution(CommandManager)
                .WithBusyNotification(BusyStack)
                .WithAsyncCancellation()
                .Build();

            Exit.UnloadOnExit(this);
        }

        protected virtual Task LoadInternal(CancellationToken token)
        {
            return Refresh(token);
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

        public virtual bool CanLoad()
        {
            return !IsLoaded
                && !_loadCommand.IsBusy
                && !_refreshCommand.IsBusy
                && !_unloadCommand.IsBusy;
        }

        protected abstract Task UnloadInternal(CancellationToken token);

        public async Task Unload(CancellationToken token)
        {
            if (!IsLoaded)
            {
                return;
            }

            using (BusyStack.GetToken())
            {
                await UnloadInternal(token).ConfigureAwait(false);
                await Dispatcher.Invoke(() => IsLoaded = false).ConfigureAwait(false);
            }
        }

        public virtual bool CanUnload()
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
                await RefreshInternal(token).ConfigureAwait(false);
            }
        }

        public virtual bool CanRefresh()
        {
            return IsLoaded
                && !_loadCommand.IsBusy
                && !_refreshCommand.IsBusy
                && !_unloadCommand.IsBusy;
        }
    }
}
