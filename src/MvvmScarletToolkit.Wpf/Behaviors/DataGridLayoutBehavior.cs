using Microsoft.Xaml.Behaviors;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace MvvmScarletToolkit.Wpf
{
    /// <summary>
    /// Behavior that keeps a bindable <see cref="DataGridLayout"/> in sync with the columns of a <see cref="DataGrid"/>. Storing the layout is up to the application.
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
    //    <mvvm:DataGridLayoutBehavior Layout="{Binding Layout}" />
    // </ i:Interaction.Behaviors>
    public sealed class DataGridLayoutBehavior : Behavior<DataGrid>
    {
        private static readonly DependencyPropertyDescriptor _visibilityDescriptor = DependencyPropertyDescriptor.FromProperty(DataGridColumn.VisibilityProperty, typeof(DataGridColumn));
        private static readonly DependencyPropertyDescriptor _widthDescriptor = DependencyPropertyDescriptor.FromProperty(DataGridColumn.WidthProperty, typeof(DataGridColumn));
        private static readonly DependencyPropertyDescriptor _summaryDescriptor = DependencyPropertyDescriptor.FromProperty(DataGridSummaryBar.FunctionProperty, typeof(DataGridColumn));

        private readonly List<DataGridColumn> _columns = [];

        private bool _isUpdating;
        private bool _isCapturePending;

        public DataGridLayout? Layout
        {
            get { return (DataGridLayout?)GetValue(LayoutProperty); }
            set { SetValue(LayoutProperty, value); }
        }

        public static readonly DependencyProperty LayoutProperty = DependencyProperty.Register(
            nameof(Layout),
            typeof(DataGridLayout),
            typeof(DataGridLayoutBehavior),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnLayoutChanged));

        private static void OnLayoutChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DataGridLayoutBehavior behavior)
            {
                behavior.ApplyLayout();
            }
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.Loaded += OnLoaded;
            AssociatedObject.ColumnReordered += OnLayoutChanged;
            AssociatedObject.Sorting += OnLayoutChanged;
            AssociatedObject.Columns.CollectionChanged += OnColumnsChanged;

            Track();
            ApplyLayout();
        }

        protected override void OnDetaching()
        {
            AssociatedObject.Loaded -= OnLoaded;
            AssociatedObject.ColumnReordered -= OnLayoutChanged;
            AssociatedObject.Sorting -= OnLayoutChanged;
            AssociatedObject.Columns.CollectionChanged -= OnColumnsChanged;

            Untrack();

            base.OnDetaching();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            ApplyLayout();
        }

        private void OnColumnsChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            Untrack();
            Track();
            ApplyLayout();
        }

        private void Track()
        {
            foreach (var column in AssociatedObject.Columns)
            {
                _visibilityDescriptor.AddValueChanged(column, OnLayoutChanged);
                _widthDescriptor.AddValueChanged(column, OnLayoutChanged);
                _summaryDescriptor.AddValueChanged(column, OnLayoutChanged);
                _columns.Add(column);
            }
        }

        private void Untrack()
        {
            // the descriptors keep strong references to their handlers
            foreach (var column in _columns)
            {
                _visibilityDescriptor.RemoveValueChanged(column, OnLayoutChanged);
                _widthDescriptor.RemoveValueChanged(column, OnLayoutChanged);
                _summaryDescriptor.RemoveValueChanged(column, OnLayoutChanged);
            }

            _columns.Clear();
        }

        private void ApplyLayout()
        {
            var layout = Layout;
            if (_isUpdating || layout is null || AssociatedObject is null)
            {
                return;
            }

            _isUpdating = true;
            try
            {
                AssociatedObject.ApplyLayout(layout);
            }
            finally
            {
                _isUpdating = false;
            }
        }

        private void OnLayoutChanged(object? sender, EventArgs e)
        {
            if (_isUpdating || _isCapturePending || AssociatedObject is null)
            {
                return;
            }

            // resizing a column raises lots of changes and the sorting event is raised before the sort is applied
            _isCapturePending = true;
            AssociatedObject.Dispatcher.BeginInvoke(Capture, DispatcherPriority.Background);
        }

        private void Capture()
        {
            _isCapturePending = false;

            if (AssociatedObject is null)
            {
                return;
            }

            _isUpdating = true;
            try
            {
                SetCurrentValue(LayoutProperty, AssociatedObject.CaptureLayout());
            }
            finally
            {
                _isUpdating = false;
            }
        }
    }
}
