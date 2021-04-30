using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;

namespace MvvmScarletToolkit.Wpf
{
    public sealed class ToastService : IToastService
    {
        private readonly ObservableCollection<ToastViewModel> _toasts;

        private ToastNotificationHostWindow? _host;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ReadOnlyObservableCollection<ToastViewModel> Toasts { get; }

        public ToastService()
        {
            _toasts = new ObservableCollection<ToastViewModel>();
            Toasts = new ReadOnlyObservableCollection<ToastViewModel>(_toasts);

            _toasts.CollectionChanged += OnCollectionChanged;
        }

        public void Show(string title, string body, ToastType toastType, TimeSpan displayTime, Rect? origin, bool isPersistent)
        {
            if (_host is null)
            {
                _host = new ToastNotificationHostWindow()
                {
                    DataContext = this,
                };

                _host.Show();
                _host.Closing += OnHostClosing;
                _host.Loaded += OnLoaded;
            }

            Reposition(_host, origin);

            if (!_host.IsVisible)
            {
                _host.Visibility = Visibility.Visible;
            }

            _toasts.Add(new ToastViewModel(_toasts, title, body, toastType, isPersistent, displayTime));
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // reposition here aswell?
        }

        private void Reposition(Window window, Rect? origin)
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

            if (sender is ToastNotificationHostWindow host)
            {
                host.Closing -= OnHostClosing;
            }
        }

        private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove && _toasts.Count == 0)
            {
                var host = _host;
                if (host is null)
                {
                    return;
                }

                host.Visibility = Visibility.Collapsed;
            }
        }
    }
}
