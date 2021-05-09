using MvvmScarletToolkit.Observables;
using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace MvvmScarletToolkit.Wpf
{
    public sealed class ToastService : ViewModelListBase<IToast>, IToastService
    {
        private static readonly Lazy<ToastService> _default = new Lazy<ToastService>(() => new ToastService(ScarletCommandBuilder.Default, new ToastServiceConfiguration()));

        public static IToastService Default => _default.Value;

        private Window? _host;
        private DispatcherTimer? _closingTimer;

        public string WindowStyleKey { get; }
        public int WindowOffset { get; }
        public TimeSpan WindowCloseDelay { get; }
        public TimeSpan ToastCloseDelay { get; }
        public Rect? Origin { get; set; }

        public ToastService(IScarletCommandBuilder commandBuilder, ToastServiceConfiguration configuration)
            : base(commandBuilder)
        {
            if (configuration is null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            if (configuration.WindowStyleKey is null)
            {
                throw new ArgumentNullException(nameof(configuration.WindowStyleKey));
            }

            WindowStyleKey = configuration.WindowStyleKey;
            WindowOffset = configuration.WindowOffset;

            WindowCloseDelay = configuration.WindowCloseDelay;
            ToastCloseDelay = configuration.ToastCloseDelay;
        }

        public async Task Show(IToast toast)
        {
            if (_host is null)
            {
                _host = new Window()
                {
                    DataContext = this,
                };

                var obj = Application.Current.FindResource(WindowStyleKey);
                if (obj is Style style && style.TargetType == typeof(Window))
                {
                    _host.Style = style;
                }

                _host.Closing += OnHostClosing;
                _host.Loaded += OnLoaded;
                _host.Show();
            }

            Reposition(_host, Origin, WindowOffset);

            if (!_host.IsVisible)
            {
                _host.Visibility = Visibility.Visible;
            }

            await Add(toast).ConfigureAwait(false);

            void OnLoaded(object sender, RoutedEventArgs e)
            {
                Reposition(_host, Origin, WindowOffset);
            }
        }

        private static void Reposition(Window window, Rect? origin, int offset)
        {
            var area = origin ?? SystemParameters.WorkArea;

            // Display the toast at the top right of the area.
            window.Left = area.Right - window.Width - offset;
            window.Top = area.Top + offset;

            // set Height to screen height, so that animations are always smooth
            window.MinHeight = area.Height - (2 * offset);
            window.Height = window.MinHeight;
        }

        private void OnHostClosing(object? sender, EventArgs e)
        {
            _host = null;

            if (sender is Window host)
            {
                host.Closing -= OnHostClosing;
            }
        }

        protected override void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove && Count == 0)
            {
                var host = _host;
                if (host is null)
                {
                    return;
                }

                host.Visibility = Visibility.Hidden;

                if (_closingTimer is null)
                {
                    _closingTimer = new DispatcherTimer()
                    {
                        Interval = WindowCloseDelay,
                    };
                    _closingTimer.Tick += OnTimerTick;
                }

                _closingTimer.Start();
            }
            else
            {
                _closingTimer?.Stop();
            }

            async void OnTimerTick(object? sender, EventArgs e)
            {
                _closingTimer?.Stop();
                _closingTimer = null;

                var host = _host;
                if (host is null)
                {
                    return;
                }

                await Dispatcher.Invoke(() => host.Close()).ConfigureAwait(false);
            }
        }
    }
}
