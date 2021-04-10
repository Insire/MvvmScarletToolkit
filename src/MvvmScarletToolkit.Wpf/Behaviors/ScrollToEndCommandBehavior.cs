using Microsoft.Xaml.Behaviors;
using System;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MvvmScarletToolkit.Wpf
{
    /// <summary>
    /// Behavior that enables executing a command, when a scrollviewer has been scrolled to its vertical or horizontal end
    /// </summary>
    /// <remarks>
    /// required namespaces:
    /// <list type="bullet">
    /// <item>
    /// <description>xmlns:i="http://schemas.microsoft.com/xaml/behaviors"</description>
    /// </item>
    /// <item>
    /// <description>xmlns:mvvm="http://SoftThorn.MvvmScarletToolkit.com/winfx/xaml/shared"</description>
    /// </item>
    /// </list>
    /// </remarks>
    // usage:
    // <i:Interaction.Behaviors>
    //  <mvvm:ScrollToEndCommandBehavior Command = "{Binding NextCommand}" Interval="00:00:00.250" />
    // </ i:Interaction.Behaviors>
#if NET5_0_OR_GREATER
    [System.Runtime.Versioning.SupportedOSPlatform("windows7.0")]
#endif

    public sealed class ScrollToEndCommandBehavior : Behavior<FrameworkElement>
    {
        public static readonly DependencyProperty IntervalProperty = DependencyProperty.Register(
            nameof(Interval),
            typeof(TimeSpan),
            typeof(ScrollToEndCommandBehavior),
            new PropertyMetadata(TimeSpan.FromMilliseconds(250)));

        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
            nameof(Command),
            typeof(ICommand),
            typeof(ScrollToEndCommandBehavior),
            new PropertyMetadata(default(ICommand)));

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public TimeSpan Interval
        {
            get { return (TimeSpan)GetValue(IntervalProperty); }
            set { SetValue(IntervalProperty, value); }
        }

        private readonly IScheduler _scheduler;

        private IDisposable? _disposable;

        private event EventHandler? CommandExecutionRequested;

        public ScrollToEndCommandBehavior()
        {
            _scheduler = new SynchronizationContextScheduler(SynchronizationContext.Current!);
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.Loaded -= OnLoaded;
            _disposable?.Dispose();

            _disposable = Observable.FromEventPattern(
                fsHandler => CommandExecutionRequested += fsHandler,
                fsHandler => CommandExecutionRequested -= fsHandler)
                .ObserveOn(TaskPoolScheduler.Default)
                .Sample(Interval)
                .ObserveOn(_scheduler)
                .Subscribe(_ => Command?.Execute(null));

            AssociatedObject.Loaded += OnLoaded;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            AssociatedObject.Loaded -= OnLoaded;
            _disposable?.Dispose();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            var scrollViewer = AssociatedObject.FindVisualChildrenBreadthFirstOrSelf<ScrollViewer>().FirstOrDefault();
            if (scrollViewer is null)
            {
                throw new InvalidOperationException("ScrollViewer not found.");
            }
            else
            {
                if (scrollViewer.VerticalScrollBarVisibility == ScrollBarVisibility.Visible || scrollViewer.VerticalScrollBarVisibility == ScrollBarVisibility.Auto)
                {
                    var propertyDescriptor = System.ComponentModel.DependencyPropertyDescriptor.FromProperty(ScrollViewer.VerticalOffsetProperty, typeof(ScrollViewer));
                    propertyDescriptor?.RemoveValueChanged(scrollViewer, OnVerticalOffsetChanged);
                    propertyDescriptor?.AddValueChanged(scrollViewer, OnVerticalOffsetChanged);

                    void OnVerticalOffsetChanged(object? sender, EventArgs args)
                    {
                        var atBottom = scrollViewer.VerticalOffset >= scrollViewer.ScrollableHeight;

                        if (atBottom)
                        {
                            CommandExecutionRequested?.Invoke(scrollViewer, EventArgs.Empty);
                        }
                    }
                }

                if (scrollViewer.HorizontalScrollBarVisibility == ScrollBarVisibility.Visible || scrollViewer.VerticalScrollBarVisibility == ScrollBarVisibility.Auto)
                {
                    var propertyDescriptor = System.ComponentModel.DependencyPropertyDescriptor.FromProperty(ScrollViewer.HorizontalOffsetProperty, typeof(ScrollViewer));
                    propertyDescriptor?.RemoveValueChanged(scrollViewer, OnHorizontalOffsetChanged);
                    propertyDescriptor?.AddValueChanged(scrollViewer, OnHorizontalOffsetChanged);

                    void OnHorizontalOffsetChanged(object? sender, EventArgs args)
                    {
                        var atBottom = scrollViewer.HorizontalOffset >= scrollViewer.ScrollableWidth;

                        if (atBottom)
                        {
                            CommandExecutionRequested?.Invoke(scrollViewer, EventArgs.Empty);
                        }
                    }
                }
            }
        }
    }
}
