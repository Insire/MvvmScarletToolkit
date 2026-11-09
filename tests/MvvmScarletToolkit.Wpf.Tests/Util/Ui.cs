using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
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

        /// <summary>
        /// A scrolling list using <paramref name="panel"/>. Not a ListBox, the bindings of its containers fail
        /// while they are being generated, which <c>DataGridStylesTests</c> would notice across threads.
        /// </summary>
        public static ItemsControl CreateItemsControl(FrameworkElementFactory panel, System.Collections.IEnumerable items)
        {
            var scrollViewer = new FrameworkElementFactory(typeof(ScrollViewer));
            scrollViewer.SetValue(ScrollViewer.CanContentScrollProperty, true);
            scrollViewer.SetValue(ScrollViewer.HorizontalScrollBarVisibilityProperty, ScrollBarVisibility.Auto);
            scrollViewer.SetValue(ScrollViewer.VerticalScrollBarVisibilityProperty, ScrollBarVisibility.Auto);
            scrollViewer.AppendChild(new FrameworkElementFactory(typeof(ItemsPresenter)));

            return new ItemsControl
            {
                ItemsPanel = new ItemsPanelTemplate(panel),
                ItemsSource = items,
                Template = new ControlTemplate(typeof(ItemsControl)) { VisualTree = scrollViewer },
            };
        }

        /// <summary>
        /// Lets the dispatcher run until <paramref name="condition"/> is met. Fails only after <see cref="TestTimeouts.Hang"/>, or when the test is cancelled.
        /// </summary>
        public static async Task WaitUntil(Func<bool> condition)
        {
            var cancellationToken = TestContext.Current.CancellationToken;
            var watch = Stopwatch.StartNew();
            while (!condition())
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (watch.Elapsed > TestTimeouts.Hang)
                {
                    throw new TimeoutException($"The condition was not met within {TestTimeouts.Hang}.");
                }

                await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
                await Task.Delay(5, cancellationToken);
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
