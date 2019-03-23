using MvvmScarletToolkit.Abstractions;
using MvvmScarletToolkit.Commands;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Observables
{
    public abstract class ViewModelBase : ObservableObject
    {
        protected readonly IBusyStack BusyStack;
        protected readonly CommandBuilder CommandBuilder;
        protected readonly IScarletCommandManager CommandManager;
        protected readonly IScarletDispatcher Dispatcher;

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
        public virtual ConcurrentCommandBase LoadCommand { get; }

        [Bindable(true, BindingDirection.OneWay)]
        public virtual ConcurrentCommandBase RefreshCommand { get; }

        [Bindable(true, BindingDirection.OneWay)]
        public virtual ConcurrentCommandBase UnloadCommand { get; }

        protected ViewModelBase(CommandBuilder commandBuilder)
        {
            CommandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
            Dispatcher = commandBuilder.Dispatcher ?? throw new ArgumentNullException(nameof(ICommandBuilderContext.Dispatcher));
            CommandManager = commandBuilder.CommandManager ?? throw new ArgumentNullException(nameof(ICommandBuilderContext.CommandManager));
            BusyStack = new ObservableBusyStack((hasItems) => IsBusy = hasItems, Dispatcher);

            LoadCommand = commandBuilder.Create(LoadInternal, CanLoad)
                                        .WithSingleExecution(CommandManager)
                                        .WithBusyNotification(BusyStack);

            RefreshCommand = commandBuilder.Create(RefreshInternal, CanRefresh)
                                            .WithSingleExecution(CommandManager)
                                            .WithBusyNotification(BusyStack);

            UnloadCommand = commandBuilder.Create(UnloadInternal, CanUnload)
                                          .WithSingleExecution(CommandManager)
                                          .WithBusyNotification(BusyStack);
        }

        // overriding here, instead of adding Dispatcher call to base class,
        // since the ObservableObject class, doesnt know anything about the concurrent usecases,
        // that i introduce here and higher up the inheritance tree
        protected sealed override async void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            await Dispatcher.Invoke(() => base.OnPropertyChanged(propertyName)).ConfigureAwait(false);
        }

        protected abstract Task Load(CancellationToken token);

        protected virtual async Task LoadInternal(CancellationToken token)
        {
            await Load(token).ConfigureAwait(false);

            IsLoaded = true;
        }

        protected virtual bool CanLoad()
        {
            return !IsLoaded && !IsBusy;
        }

        protected abstract Task Unload(CancellationToken token);

        protected virtual async Task UnloadInternal(CancellationToken token)
        {
            await Unload(token).ConfigureAwait(false);

            IsLoaded = false;
        }

        protected virtual bool CanUnload()
        {
            return IsLoaded && !IsBusy;
        }

        protected abstract Task Refresh(CancellationToken token);

        protected virtual Task RefreshInternal(CancellationToken token)
        {
            return Refresh(token);
        }

        protected virtual bool CanRefresh()
        {
            return IsLoaded && !IsBusy;
        }
    }
}
