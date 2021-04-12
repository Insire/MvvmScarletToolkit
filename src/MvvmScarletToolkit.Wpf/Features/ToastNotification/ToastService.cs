using System;
using System.Windows;

namespace MvvmScarletToolkit.Wpf
{
    public sealed class ToastService : IToastService
    {
        private ToastNotificationHost? _host;

        public void Show(string title, string message, ToastType toastType, TimeSpan displayTime, Rect? origin = null, bool isPersistent = false)
        {
            //Show the host window
            ShowHost(origin);

            //Create a new toast control
            var notification = new ToastNotification()
            {
                Title = title,
                Message = message,
                ToastType = toastType,
                IsPersistent = isPersistent
            };

            //Create a toast and accompanying dispatcher timer.
            var toast = new Toast(notification);
            toast.ToastClosing += Toast_ToastClosing;

            //Add the toast to the host window
            _host?.Toasts?.Add(notification);

            //display it for X seconds
            toast.Show(displayTime);
        }

        private void ShowHost(Rect? origin)
        {
            //If the current host window is null, create it.
            if (_host == null)
            {
                _host = new ToastNotificationHost()
                {
                    DisplayOrigin = origin
                };

                _host.Show();
                _host.Closed += _HOST_Closed;
            }
            //Otherwise, set it's display location and origin.
            else
            {
                _host.DisplayOrigin = origin;
            }

            _host.Reposition();

            //Display the window
            if (!_host.IsVisible)
            {
                _host.Visibility = Visibility.Visible;
            }
        }

        private void Toast_ToastClosing(object sender, ToastNotification e)
        {
            //Remove the toast from the host window
            _host?.Toasts?.Remove(e);

            //If there are no more toasts to show, then close the host window
            if (_host?.Toasts?.Count == 0)
            {
                _host.Visibility = Visibility.Collapsed;
            }
        }

        private void _HOST_Closed(object sender, EventArgs e)
        {
            //Reset the host.
            _host.Closed -= _HOST_Closed;
            _host = null;
        }
    }
}
