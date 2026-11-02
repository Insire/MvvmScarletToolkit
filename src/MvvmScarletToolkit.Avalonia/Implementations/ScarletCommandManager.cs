using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;

namespace MvvmScarletToolkit
{
    public sealed class ScarletCommandManager : IScarletCommandManager
    {
        private static readonly Lazy<ScarletCommandManager> _default = new Lazy<ScarletCommandManager>(() => new ScarletCommandManager());

        public static IScarletCommandManager Default => _default.Value;

        private int _isScheduled;

        public event EventHandler? RequerySuggested;

        public ScarletCommandManager()
        {
            // Avalonia has no CommandManager, this requeries after user input like the one in WPF does.
            // Tunneling starts at the TopLevel, so every input is observed exactly once, handled or not
            InputElement.PointerReleasedEvent.AddClassHandler<TopLevel>((_, _) => InvalidateRequerySuggested(), RoutingStrategies.Tunnel, handledEventsToo: true);
            InputElement.KeyUpEvent.AddClassHandler<TopLevel>((_, _) => InvalidateRequerySuggested(), RoutingStrategies.Tunnel, handledEventsToo: true);
        }

        /// <summary>
        /// Can be called from any thread, handlers run on the UI thread after the current input has been processed. Several calls in a row raise a single event.
        /// </summary>
        public void InvalidateRequerySuggested()
        {
            if (Interlocked.Exchange(ref _isScheduled, 1) == 1)
            {
                return;
            }

            Dispatcher.UIThread.Post(() =>
            {
                Volatile.Write(ref _isScheduled, 0);
                RequerySuggested?.Invoke(this, EventArgs.Empty);
            }, DispatcherPriority.Background);
        }
    }
}
