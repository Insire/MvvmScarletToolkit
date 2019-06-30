using MvvmScarletToolkit.Abstractions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MvvmScarletToolkit.Observables
{
    /// <summary>
    /// Generic ViewModelBase that provides common services required for MVVM
    /// </summary>
    public abstract class ViewModelBase<TModel> : ViewModelBase
    {
        private TModel _model;
        [Bindable(true, BindingDirection.OneWay)]
        public TModel Model
        {
            get { return _model; }
            protected set { SetValue(ref _model, value); }
        }

        protected ViewModelBase(ICommandBuilder commandBuilder)
            : base(commandBuilder)
        {
        }

        protected ViewModelBase(ICommandBuilder commandBuilder, TModel model)
            : this(commandBuilder)
        {
            Model = model;
        }
    }

    /// <summary>
    /// ViewModelBase that provides common services required for MVVM
    /// </summary>
    public abstract class ViewModelBase : INotifyPropertyChanged, IDisposable
    {
        protected readonly IBusyStack BusyStack;
        protected readonly ICommandBuilder CommandBuilder;
        protected readonly IScarletCommandManager CommandManager;
        protected readonly IScarletDispatcher Dispatcher;
        protected readonly IScarletMessenger Messenger;
        protected readonly IExitService Exit;
        protected readonly IWeakEventManager<INotifyPropertyChanged, PropertyChangedEventArgs> WeakEventManager;

        private bool _disposed;

        public event PropertyChangedEventHandler PropertyChanged;

        private bool _isBusy;
        [Bindable(true, BindingDirection.OneWay)]
        public bool IsBusy
        {
            get { return _isBusy; }
            protected set { SetValue(ref _isBusy, value); }
        }

        protected ViewModelBase(ICommandBuilder commandBuilder)
        {
            CommandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
            Dispatcher = commandBuilder.Dispatcher ?? throw new ArgumentNullException(nameof(ICommandBuilder.Dispatcher));
            CommandManager = commandBuilder.CommandManager ?? throw new ArgumentNullException(nameof(ICommandBuilder.CommandManager));
            Messenger = commandBuilder.Messenger ?? throw new ArgumentNullException(nameof(ICommandBuilder.Messenger));
            Exit = commandBuilder.Exit ?? throw new ArgumentNullException(nameof(ICommandBuilder.Exit));
            WeakEventManager = commandBuilder.WeakEventManager ?? throw new ArgumentNullException(nameof(ICommandBuilder.WeakEventManager));

            BusyStack = new ObservableBusyStack((hasItems) => IsBusy = hasItems, Dispatcher);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetValue<T>(ref T field, T value, [CallerMemberName]string propertyName = null)
        {
            return SetValue(ref field, value, null, null, propertyName);
        }

        protected bool SetValue<T>(ref T field, T value, Action OnChanged, [CallerMemberName]string propertyName = null)
        {
            return SetValue(ref field, value, null, OnChanged, propertyName);
        }

        protected virtual bool SetValue<T>(ref T field, T value, Action OnChanging, Action OnChanged, [CallerMemberName]string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            OnChanging?.Invoke();

            field = value;
            OnPropertyChanged(propertyName);

            OnChanged?.Invoke();

            return true;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            if (disposing)
            {
                (BusyStack as IDisposable)?.Dispose();
            }
        }
    }
}
