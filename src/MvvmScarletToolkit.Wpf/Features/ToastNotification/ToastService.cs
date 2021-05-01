using MvvmScarletToolkit.Observables;
using System;
using System.Collections.Specialized;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace MvvmScarletToolkit.Wpf
{
    public sealed class ToastService : BusinessViewModelListBase<ToastViewModel>, IToastService
    {
        private static readonly Lazy<ToastService> _default = new Lazy<ToastService>(() => new ToastService(ScarletCommandBuilder.Default));

        public static IToastService Default => _default.Value;

        private Window? _host;

        public ToastService(IScarletCommandBuilder commandBuilder)
            : base(commandBuilder)
        {
        }

        public async Task Show(string title, string body, ToastType toastType, TimeSpan displayTime, Rect? origin, bool isPersistent)
        {
            if (_host is null)
            {
                _host = new Window()
                {
                    DataContext = this,
                };

                var obj = Application.Current.FindResource("DefaultToastWindowStyle");
                if (obj is Style style && style.TargetType == typeof(Window))
                {
                    _host.Style = style;
                }

                _host.Closing += OnHostClosing;
                _host.Loaded += OnLoaded;
                _host.Show();
            }

            Reposition(_host, origin);

            if (!_host.IsVisible)
            {
                _host.Visibility = Visibility.Visible;
            }

            await Add(new ToastViewModel(this, title, body, toastType, isPersistent, displayTime)).ConfigureAwait(false);

            void OnLoaded(object sender, RoutedEventArgs e)
            {
                Reposition(_host, origin);
            }
        }

        private static void Reposition(Window window, Rect? origin)
        {
            const int offset = 12;

            var area = origin ?? SystemParameters.WorkArea;

            //Display the toast at the top right of the area.
            window.Left = area.Right - window.Width - offset;
            window.Top = area.Top + offset;
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
            }
        }

        protected override Task RefreshInternal(CancellationToken token)
        {
            return Task.CompletedTask;
        }

        protected override async Task UnloadInternal(CancellationToken token)
        {
            await base.UnloadInternal(token).ConfigureAwait(false);

            var host = _host;
            if (host is null)
            {
                return;
            }

            await Dispatcher.Invoke(() => host.Close()).ConfigureAwait(false);
        }
    }
}
