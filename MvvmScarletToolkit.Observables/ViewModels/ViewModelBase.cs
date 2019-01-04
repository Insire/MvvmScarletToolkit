using MvvmScarletToolkit.Abstractions;
using MvvmScarletToolkit.Commands;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Observables
{
    public abstract class ViewModelBase : ObservableObject
    {
        protected readonly ObservableBusyStack BusyStack;
        protected readonly ICommandManager CommandManager;

        private bool _isBusy;
        [Bindable(true, BindingDirection.OneWay)]
        public bool IsBusy
        {
            get { return _isBusy; }
            protected set { SetValue(ref _isBusy, value); }
        }

        private bool _isLoaded;
        [Bindable(true, BindingDirection.OneWay)]
        public bool IsLoaded
        {
            get { return _isLoaded; }
            protected set { SetValue(ref _isLoaded, value); }
        }

        [Bindable(true, BindingDirection.OneWay)]
        public virtual IExtendedAsyncCommand LoadCommand { get; }

        [Bindable(true, BindingDirection.OneWay)]
        public virtual IExtendedAsyncCommand RefreshCommand { get; }

        [Bindable(true, BindingDirection.OneWay)]
        public virtual IExtendedAsyncCommand UnloadCommand { get; }

        protected ViewModelBase(ICommandManager commandManager)
        {
            CommandManager = commandManager ?? throw new ArgumentNullException(nameof(commandManager));
            BusyStack = new ObservableBusyStack((hasItems) => IsBusy = hasItems);

            LoadCommand = AsyncCommand.Create(LoadInternal, CanLoad, commandManager).AsSequential();
            RefreshCommand = AsyncCommand.Create(RefreshInternal, CanRefresh, commandManager);
            UnloadCommand = AsyncCommand.Create(UnloadInternalAsync, CanUnload, commandManager).AsSequential();
        }

        protected abstract Task LoadInternal(CancellationToken token);

        protected virtual bool CanLoad()
        {
            return !IsLoaded;
        }

        protected abstract Task UnloadInternalAsync();

        protected virtual bool CanUnload()
        {
            return IsLoaded;
        }

        protected abstract Task RefreshInternal(CancellationToken token);

        protected virtual bool CanRefresh()
        {
            return IsLoaded;
        }
    }
}
