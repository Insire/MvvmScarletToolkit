using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace MvvmScarletToolkit.Avalonia.Samples.Features
{
    /// <summary>
    /// Shows notifications in the corner of the primary screen, like the toast service of the WPF package.
    /// <see cref="WindowNotificationManager"/> can only draw inside of its window, so this hosts its cards in a window of their own.
    /// </summary>
    public sealed class DesktopToastManager
    {
        private const int MaxItems = 5;
        // together with the margin of the cards, which leaves room for their shadow, this is the distance the toast service of the WPF package keeps from the screen edges
        private const int ScreenMargin = 4;
        private const double ToastWidth = 350;

        private readonly StackPanel _cards = new() { Spacing = 4, Margin = new Thickness(8) };

        private Window? _window;

        public void Show(INotification notification)
        {
            var window = _window ??= CreateWindow();

            var card = new NotificationCard
            {
                // Avalonia only knows how to display a notification inside of a WindowNotificationManager
                Content = CreateContent(notification),
                NotificationType = notification.Type,
                Tag = notification,
            };

            card.NotificationClosed += OnNotificationClosed;

            void OnNotificationClosed(object? sender, RoutedEventArgs e)
            {
                card.NotificationClosed -= OnNotificationClosed;
                _cards.Children.Remove(card);
                notification.OnClose?.Invoke();

                if (_cards.Children.Count == 0)
                {
                    window.Hide();
                }
            }

            _cards.Children.Add(card);

            // the newest ones stay. Without an animation, cards that are closing would still take up space, and the window would outgrow the screen
            while (_cards.Children.Count > MaxItems)
            {
                var oldest = (NotificationCard)_cards.Children[0];

                _cards.Children.RemoveAt(0);
                (oldest.Tag as INotification)?.OnClose?.Invoke();
            }

            if (!window.IsVisible)
            {
                window.Show();
            }

            // a zero expiration means that the notification stays until it is closed
            if (notification.Expiration != TimeSpan.Zero)
            {
                _ = CloseAfter(card, notification.Expiration);
            }
        }

        private static StackPanel CreateContent(INotification notification)
        {
            return new StackPanel
            {
                Margin = new Thickness(12),
                Spacing = 4,
                Children =
                {
                    new TextBlock { FontWeight = FontWeight.SemiBold, Text = notification.Title, TextWrapping = TextWrapping.Wrap },
                    new TextBlock { Text = notification.Message, TextWrapping = TextWrapping.Wrap },
                },
            };
        }

        private static async Task CloseAfter(NotificationCard card, TimeSpan expiration)
        {
            await Task.Delay(expiration);

            card.Close();
        }

        private Window CreateWindow()
        {
            var window = new Window
            {
                Background = Brushes.Transparent,
                CanResize = false,
                Content = _cards,
                ShowActivated = false,
                ShowInTaskbar = false,
                SizeToContent = SizeToContent.Height,
                Topmost = true,
                TransparencyLevelHint = [WindowTransparencyLevel.Transparent],
                Width = ToastWidth,
                WindowDecorations = WindowDecorations.None,
            };

            // the width in pixels depends on the scaling of the screen, which is only known once the window has a size
            window.SizeChanged += (_, _) => UpdatePosition(window);

            return window;
        }

        private static void UpdatePosition(Window window)
        {
            if (window.Screens.Primary is not { } screen)
            {
                return;
            }

            var area = screen.WorkingArea;
            var size = PixelSize.FromSize(window.Bounds.Size, screen.Scaling);

            // the top right of the screen, like the toast service of the WPF package
            window.Position = new PixelPoint(area.Right - size.Width - ScreenMargin, area.Y + ScreenMargin);
        }
    }
}
