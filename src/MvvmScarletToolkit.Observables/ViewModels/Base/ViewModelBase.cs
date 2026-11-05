using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace MvvmScarletToolkit.Observables
{
    /// <summary>
    /// BaseViewModel that serves as service aggregate and caches <see cref="INotifyPropertyChanged"/> EventArgs
    /// </summary>
    public abstract partial class ViewModelBase : ObservableRecipient, IDisposable
    {
        protected readonly IObservableBusyStack BusyStack;
        protected readonly IScarletCommandBuilder CommandBuilder;
        protected readonly IScarletCommandManager CommandManager;
        protected readonly IScarletDispatcher Dispatcher;
        protected readonly IExitService Exit;
        protected readonly IScarletEventManager<INotifyPropertyChanged, PropertyChangedEventArgs> WeakEventManager;

        [ObservableProperty, Bindable(true, BindingDirection.OneWay)] public partial bool IsBusy { get; protected set; }

        protected bool IsDisposed { get; private set; }

        protected ViewModelBase(IScarletCommandBuilder commandBuilder)
            : base(commandBuilder.Messenger)
        {
            CommandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
            Dispatcher = commandBuilder.Dispatcher ?? throw new ArgumentNullException(nameof(IScarletCommandBuilder.Dispatcher));
            CommandManager = commandBuilder.CommandManager ?? throw new ArgumentNullException(nameof(IScarletCommandBuilder.CommandManager));
            Exit = commandBuilder.Exit ?? throw new ArgumentNullException(nameof(IScarletCommandBuilder.Exit));
            WeakEventManager = commandBuilder.WeakEventManager ?? throw new ArgumentNullException(nameof(IScarletCommandBuilder.WeakEventManager));

            BusyStack = new ObservableBusyStack((hasItems) => IsBusy = hasItems);
        }

        /// <summary>
        /// Does not register <see cref="CommunityToolkit.Mvvm.Messaging.IRecipient{TMessage}"/> implementations,
        /// unlike <see cref="ObservableRecipient"/>. Register message handlers explicitly through <see cref="ObservableRecipient.Messenger"/>.
        /// </summary>
        /// <remarks>
        /// The base implementation calls <c>Messenger.RegisterAll(this)</c>, which needs reflection and is not trim or AOT safe.
        /// The attributes only mirror the overridden method and are suppressed for this implementation in <see cref="SetIsActive"/>.
        /// </remarks>
        [RequiresUnreferencedCode("Mirrors ObservableRecipient.OnActivated. This override does not use reflection.")]
        [RequiresDynamicCode("Mirrors ObservableRecipient.OnActivated. This override does not use dynamic code.")]
        protected override void OnActivated()
        {
        }

        /// <summary>
        /// Sets <see cref="ObservableRecipient.IsActive"/> without trim warnings, since <see cref="OnActivated"/> does not use reflection.
        /// </summary>
        [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "OnActivated is overridden without reflection.")]
        [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "OnActivated is overridden without dynamic code.")]
        private protected void SetIsActive(bool value)
        {
            IsActive = value;
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
                BusyStack.Dispose();
                SetIsActive(false);
            }

            IsDisposed = true;
        }
    }
}
