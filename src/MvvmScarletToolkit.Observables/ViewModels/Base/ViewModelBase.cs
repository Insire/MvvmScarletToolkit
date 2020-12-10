using Microsoft.Toolkit.Mvvm.ComponentModel;
using MvvmScarletToolkit.Abstractions;
using System;
using System.ComponentModel;

namespace MvvmScarletToolkit.Observables
{
    /// <summary>
    /// BaseViewModel that serves as service aggregate and caches <see cref="INotifyPropertyChanged"/> EventArgs
    /// </summary>
    public abstract class ViewModelBase : ObservableRecipient, IDisposable
    {
        protected readonly IObservableBusyStack BusyStack;
        protected readonly IScarletCommandBuilder CommandBuilder;
        protected readonly IScarletCommandManager CommandManager;
        protected readonly IScarletDispatcher Dispatcher;
        protected readonly IExitService Exit;
        protected readonly IScarletEventManager<INotifyPropertyChanged, PropertyChangedEventArgs> WeakEventManager;

        private bool _isBusy;
        [Bindable(true, BindingDirection.OneWay)]
        public bool IsBusy
        {
            get { return _isBusy; }
            protected set { SetProperty(ref _isBusy, value); }
        }

        protected bool IsDisposed { get; private set; }

        protected ViewModelBase(IScarletCommandBuilder commandBuilder)
#pragma warning disable CS8604 // Possible null reference argument. Yes, thats intended
            : base(commandBuilder?.Messenger)
#pragma warning restore CS8604 // Possible null reference argument.
        {
            CommandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
            Dispatcher = commandBuilder.Dispatcher ?? throw new ArgumentNullException(nameof(IScarletCommandBuilder.Dispatcher));
            CommandManager = commandBuilder.CommandManager ?? throw new ArgumentNullException(nameof(IScarletCommandBuilder.CommandManager));
            Exit = commandBuilder.Exit ?? throw new ArgumentNullException(nameof(IScarletCommandBuilder.Exit));
            WeakEventManager = commandBuilder.WeakEventManager ?? throw new ArgumentNullException(nameof(IScarletCommandBuilder.WeakEventManager));

            BusyStack = new ObservableBusyStack((hasItems) => IsBusy = hasItems, Dispatcher);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (IsDisposed)
            {
                return;
            }

            if (disposing)
            {
                BusyStack?.Dispose();
                IsActive = false;
            }

            IsDisposed = true;
        }
    }
}
