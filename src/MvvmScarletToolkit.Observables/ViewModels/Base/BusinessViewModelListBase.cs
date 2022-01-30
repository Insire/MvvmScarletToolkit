using MvvmScarletToolkit.Commands;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MvvmScarletToolkit.Observables
{
    /// <summary>
    /// Collection ViewModelBase that bootstraps loading, unloading and refreshing of its content
    /// </summary>
    /// <typeparam name="TViewModel"></typeparam>
    public abstract class BusinessViewModelListBase<TViewModel> : ViewModelListBase<TViewModel>, IBusinessViewModelListBase<TViewModel>
        where TViewModel : class, INotifyPropertyChanged
    {
        private bool _isLoaded;
        /// <summary>
        /// Indicates whether <see cref="Load"/> has been called already
        /// </summary>
        [Bindable(true, BindingDirection.OneWay)]
        public bool IsLoaded
        {
            get { return _isLoaded; }
            protected set { SetProperty(ref _isLoaded, value); }
        }

        private readonly ConcurrentCommandBase _loadCommand;
        /// <summary>
        /// Executes <see cref="Refresh"/> unless, <see cref="Load"/> has already been called
        /// </summary>
        [Bindable(true, BindingDirection.OneWay)]
        public virtual ICommand LoadCommand => _loadCommand;

        private readonly ConcurrentCommandBase _refreshCommand;
        /// <summary>
        /// Clears <see cref="Items"/> to add new instances
        /// </summary>
        [Bindable(true, BindingDirection.OneWay)]
        public virtual ICommand RefreshCommand => _refreshCommand;

        private readonly ConcurrentCommandBase _unloadCommand;
        /// <summary>
        /// Undoes <see cref="Load"/>. Clears all instances by default.
        /// </summary>
        [Bindable(true, BindingDirection.OneWay)]
        public virtual ICommand UnloadCommand => _unloadCommand;

        protected BusinessViewModelListBase(in IScarletCommandBuilder commandBuilder)
            : base(commandBuilder)
        {
            _loadCommand = commandBuilder
                .Create(Load, CanLoad)
                .WithSingleExecution()
                .WithBusyNotification(BusyStack)
                .WithAsyncCancellation()
                .Build();

            _refreshCommand = commandBuilder
                .Create(Refresh, CanRefresh)
                .WithSingleExecution()
                .WithBusyNotification(BusyStack)
                .WithAsyncCancellation()
                .Build();

            _unloadCommand = commandBuilder
                .Create(Unload, CanUnload)
                .WithSingleExecution()
                .WithBusyNotification(BusyStack)
                .WithAsyncCancellation()
                .Build();

            Exit.UnloadOnExit(this);
        }

        protected virtual Task LoadInternal(CancellationToken token)
        {
            return RefreshInternal(token);
        }

        public async Task Load(CancellationToken token)
        {
            if (IsDisposed)
            {
                return;
            }

            using (BusyStack.GetToken())
            {
                await LoadInternal(token).ConfigureAwait(false);
                await Dispatcher.Invoke(() => IsLoaded = true).ConfigureAwait(false);
                await Dispatcher.Invoke(() => IsActive = true).ConfigureAwait(false);
            }
        }

        public virtual bool CanLoad()
        {
            return !IsDisposed
                && !IsLoaded
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
            if (IsDisposed)
            {
                return;
            }

            using (BusyStack.GetToken())
            {
                await UnloadInternal(token).ConfigureAwait(false);
                await Dispatcher.Invoke(() => IsLoaded = false).ConfigureAwait(false);
                await Dispatcher.Invoke(() => IsActive = false).ConfigureAwait(false);
            }
        }

        public virtual bool CanUnload()
        {
            return !IsDisposed
                && !IsLoaded
                && !_loadCommand.IsBusy
                && !_refreshCommand.IsBusy
                && !_unloadCommand.IsBusy;
        }

        protected abstract Task RefreshInternal(CancellationToken token);

        public async Task Refresh(CancellationToken token)
        {
            if (IsDisposed)
            {
                return;
            }

            using (BusyStack.GetToken())
            {
                await Clear(token).ConfigureAwait(false);
                await RefreshInternal(token).ConfigureAwait(false);
            }
        }

        public virtual bool CanRefresh()
        {
            return !IsDisposed
                && IsLoaded
                && !_loadCommand.IsBusy
                && !_refreshCommand.IsBusy
                && !_unloadCommand.IsBusy;
        }

        protected override async void Dispose(bool disposing)
        {
            if (IsDisposed)
            {
                return;
            }

            if (disposing)
            {
                if (IsLoaded && !IsBusy)
                {
                    await Unload(CancellationToken.None).ConfigureAwait(false);
                }
            }

            base.Dispose(disposing);
        }
    }
}
