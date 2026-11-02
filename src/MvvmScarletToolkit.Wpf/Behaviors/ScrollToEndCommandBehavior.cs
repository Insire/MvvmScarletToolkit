using Microsoft.Xaml.Behaviors;
using System.ComponentModel;
using System.Diagnostics;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

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
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        public TimeSpan Interval
        {
            get => (TimeSpan)GetValue(IntervalProperty);
            set => SetValue(IntervalProperty, value);
        }

        private static readonly DependencyPropertyDescriptor _verticalOffsetDescriptor = DependencyPropertyDescriptor.FromProperty(ScrollViewer.VerticalOffsetProperty, typeof(ScrollViewer));
        private static readonly DependencyPropertyDescriptor _horizontalOffsetDescriptor = DependencyPropertyDescriptor.FromProperty(ScrollViewer.HorizontalOffsetProperty, typeof(ScrollViewer));

        private IDisposable? _disposable;
        private ScrollViewer? _scrollViewer;

        private event EventHandler? CommandExecutionRequested;

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.Loaded -= OnLoaded;
            _disposable?.Dispose();

            // created here instead of in the constructor, SynchronizationContext.Current may be null at construction time
            var scheduler = new SynchronizationContextScheduler(new DispatcherSynchronizationContext(AssociatedObject.Dispatcher));

            _disposable = Observable.FromEventPattern(
                fsHandler => CommandExecutionRequested += fsHandler,
                fsHandler => CommandExecutionRequested -= fsHandler)
                .ObserveOn(TaskPoolScheduler.Default)
                .Sample(Interval)
                .ObserveOn(scheduler)
                .Subscribe(_ => Command?.Execute(null));

            AssociatedObject.Loaded += OnLoaded;

            if (AssociatedObject.IsLoaded)
            {
                OnLoaded(AssociatedObject, new RoutedEventArgs());
            }
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            AssociatedObject.Loaded -= OnLoaded;
            DetachScrollViewer();
            _disposable?.Dispose();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // Loaded fires again e.g. after switching tabs, don't subscribe twice
            DetachScrollViewer();

            var scrollViewer = AssociatedObject.FindVisualChildrenBreadthFirstOrSelf<ScrollViewer>().FirstOrDefault();
            if (scrollViewer is null)
            {
                // throwing from an event handler would crash the application
                Trace.TraceWarning("{0}: no ScrollViewer found in {1}", nameof(ScrollToEndCommandBehavior), AssociatedObject);
                return;
            }

            _scrollViewer = scrollViewer;

            if (scrollViewer.VerticalScrollBarVisibility is ScrollBarVisibility.Visible or ScrollBarVisibility.Auto)
            {
                _verticalOffsetDescriptor.AddValueChanged(scrollViewer, OnVerticalOffsetChanged);
            }

            if (scrollViewer.HorizontalScrollBarVisibility is ScrollBarVisibility.Visible or ScrollBarVisibility.Auto)
            {
                _horizontalOffsetDescriptor.AddValueChanged(scrollViewer, OnHorizontalOffsetChanged);
            }
        }

        private void DetachScrollViewer()
        {
            if (_scrollViewer is null)
            {
                return;
            }

            _verticalOffsetDescriptor.RemoveValueChanged(_scrollViewer, OnVerticalOffsetChanged);
            _horizontalOffsetDescriptor.RemoveValueChanged(_scrollViewer, OnHorizontalOffsetChanged);
            _scrollViewer = null;
        }

        private void OnVerticalOffsetChanged(object? sender, EventArgs args)
        {
            if (sender is ScrollViewer scrollViewer && scrollViewer.VerticalOffset >= scrollViewer.ScrollableHeight)
            {
                CommandExecutionRequested?.Invoke(scrollViewer, EventArgs.Empty);
            }
        }

        private void OnHorizontalOffsetChanged(object? sender, EventArgs args)
        {
            if (sender is ScrollViewer scrollViewer && scrollViewer.HorizontalOffset >= scrollViewer.ScrollableWidth)
            {
                CommandExecutionRequested?.Invoke(scrollViewer, EventArgs.Empty);
            }
        }
    }
}
