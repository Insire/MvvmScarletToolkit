using MvvmScarletToolkit.Abstractions;
using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MvvmScarletToolkit.Observables
{
    /// <summary>
    /// BaseViewModel that serves as service aggregate and caches <see cref="INotifyPropertyChanged"/> EventArgs
    /// </summary>
    public abstract class ViewModelBase : ObservableObject, IDisposable
    {
        private static readonly ConcurrentDictionary<string, PropertyChangedEventArgs> _propertyChangedCache = new ConcurrentDictionary<string, PropertyChangedEventArgs>();

        protected readonly IObservableBusyStack BusyStack;
        protected readonly IScarletCommandBuilder CommandBuilder;
        protected readonly IScarletCommandManager CommandManager;
        protected readonly IScarletDispatcher Dispatcher;
        protected readonly IScarletMessenger Messenger;
        protected readonly IExitService Exit;
        protected readonly IScarletEventManager<INotifyPropertyChanged, PropertyChangedEventArgs> WeakEventManager;

        private bool _isBusy;
        [Bindable(true, BindingDirection.OneWay)]
        public bool IsBusy
        {
            get { return _isBusy; }
            protected set { SetValue(ref _isBusy, value); }
        }

        protected bool IsDisposed { get; private set; }

        protected ViewModelBase(IScarletCommandBuilder commandBuilder)
        {
            CommandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
            Dispatcher = commandBuilder.Dispatcher ?? throw new ArgumentNullException(nameof(IScarletCommandBuilder.Dispatcher));
            CommandManager = commandBuilder.CommandManager ?? throw new ArgumentNullException(nameof(IScarletCommandBuilder.CommandManager));
            Messenger = commandBuilder.Messenger ?? throw new ArgumentNullException(nameof(IScarletCommandBuilder.Messenger));
            Exit = commandBuilder.Exit ?? throw new ArgumentNullException(nameof(IScarletCommandBuilder.Exit));
            WeakEventManager = commandBuilder.WeakEventManager ?? throw new ArgumentNullException(nameof(IScarletCommandBuilder.WeakEventManager));

            BusyStack = new ObservableBusyStack((hasItems) => IsBusy = hasItems, Dispatcher);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected override void OnPropertyChanged([CallerMemberName] in string? propertyName = null)
        {
            OnPropertyChanged(_propertyChangedCache.GetOrAdd(propertyName ?? string.Empty, name => new PropertyChangedEventArgs(name)));
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
            }

            IsDisposed = true;
        }
    }
}
