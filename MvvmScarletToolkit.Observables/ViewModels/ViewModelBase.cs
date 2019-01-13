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
        protected readonly ObservableBusyStack BusyStack;
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
            BusyStack = new ObservableBusyStack((hasItems) => IsBusy = hasItems);

            LoadCommand = commandBuilder.Create(LoadInternal, CanLoad)
                                        .WithSingleExecution(CommandManager);

            RefreshCommand = commandBuilder.Create(RefreshInternal, CanRefresh);
            UnloadCommand = commandBuilder.Create(UnloadInternalAsync, CanUnload)
                                          .WithSingleExecution(CommandManager);
        }

        // overriding here, instead of adding Dispatcher call to base class,
        // since the ObservableObject class, doesnt know anything about the concurrent usecases,
        // that i introduce here and higher up the inheritance tree
        protected override async void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            await Dispatcher.Invoke(() => base.OnPropertyChanged(propertyName)).ConfigureAwait(false);
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
