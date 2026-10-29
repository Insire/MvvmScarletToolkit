using System.Windows;
using System.Windows.Threading;

namespace MvvmScarletToolkit.Wpf.Tests.Util
{
    /// <summary>
    /// Helpers for tests that need a live visual tree. Must be called from within <see cref="StaRunner.Run"/>.
    /// </summary>
    internal static class Ui
    {
        /// <summary>
        /// Shows <paramref name="content"/> in an off screen window, so that it is loaded and gets a layout.
        /// </summary>
        public static Window Show(object content, double width = 200, double height = 200)
        {
            var window = new Window
            {
                Content = content,
                Width = width,
                Height = height,
                Left = -10000,
                Top = -10000,
                WindowStyle = WindowStyle.None,
                ShowInTaskbar = false,
                ShowActivated = false,
            };
            window.Show();

            return window;
        }

        public static async Task WaitUntil(Func<bool> condition)
        {
            var timeout = DateTime.UtcNow.AddSeconds(5);
            while (!condition())
            {
                if (DateTime.UtcNow > timeout)
                {
                    throw new TimeoutException();
                }

                await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
                await Task.Delay(5);
            }
        }

        /// <summary>
        /// Lets queued dispatcher work, layout and rendering run.
        /// </summary>
        public static async Task Settle()
        {
            for (var i = 0; i < 10; i++)
            {
                await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
                await Task.Delay(5);
            }
        }
    }
}
