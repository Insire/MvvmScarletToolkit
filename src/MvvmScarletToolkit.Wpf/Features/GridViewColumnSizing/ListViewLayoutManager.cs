using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace MvvmScarletToolkit
{
    // Original Author: Jani Giannoudis 27 Sep 2012
    // Original License: The Code Project Open License (CPOL) 1.02
    // Original Source: https://www.codeproject.com/Articles/25058/ListView-Layout-Manager
    public sealed class ListViewLayoutManager
    {
        public static readonly DependencyProperty EnabledProperty = DependencyProperty.RegisterAttached(
            "Enabled",
            typeof(bool),
            typeof(ListViewLayoutManager),
            new FrameworkPropertyMetadata(new PropertyChangedCallback(OnEnabledChanged)));

        private const double ZeroWidthRange = 0.1;

        private GridViewColumn? _autoSizedColumn;
        private ScrollViewer? _scrollViewer;
        private Cursor? _resizeCursor;
        private bool _loaded;
        private bool _resizing;

        private ListView ListView { get; }

        private ScrollBarVisibility VerticalScrollBarVisibility { get; } = ScrollBarVisibility.Auto;

        public ListViewLayoutManager(ListView listView)
        {
            ListView = listView ?? throw new ArgumentNullException(nameof(listView));
            ListView.Loaded += ListViewLoaded;
            ListView.Unloaded += ListViewUnloaded;
        }

        /// <summary>Helper for setting <see cref="EnabledProperty"/> on <paramref name="dependencyObject"/>.</summary>
        /// <param name="dependencyObject"><see cref="DependencyObject"/> to set <see cref="EnabledProperty"/> on.</param>
        /// <param name="enabled">Enabled property value.</param>
        public static void SetEnabled(DependencyObject dependencyObject, bool enabled)
        {
            dependencyObject.SetValue(EnabledProperty, enabled);
        }

        public void Refresh()
        {
            InitColumns();
            DoResizeColumns();
        }

        private void RegisterEvents(DependencyObject? start)
        {
            if (start is null)
            {
                return;
            }

            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(start); i++)
            {
                var childVisual = VisualTreeHelper.GetChild(start, i) as Visual;
                switch (childVisual)
                {
                    case Thumb thumb:
                        RegisterThumb(thumb);
                        break;

                    case GridViewColumnHeader columnHeader:
                        columnHeader.SizeChanged += GridColumnHeaderSizeChanged;
                        break;

                    case ScrollViewer scrollViewer when _scrollViewer is null:
                        RegisterScrollViewer(scrollViewer);
                        break;
                }

                RegisterEvents(childVisual);
            }
        }

        private void RegisterThumb(Thumb thumb)
        {
            var gridViewColumn = FindParentColumn(thumb);
            if (gridViewColumn is null)
            {
                return;
            }

            if (IsResizeSuppressed(gridViewColumn))
            {
                thumb.SetCurrentValue(UIElement.IsHitTestVisibleProperty, false);
                return;
            }

            thumb.PreviewMouseMove += ThumbPreviewMouseMove;
            thumb.PreviewMouseLeftButtonDown += ThumbPreviewMouseLeftButtonDown;
            DependencyPropertyDescriptor.FromProperty(
                GridViewColumn.WidthProperty,
                typeof(GridViewColumn)).AddValueChanged(gridViewColumn, GridColumnWidthChanged);
        }

        private void RegisterScrollViewer(ScrollViewer scrollViewer)
        {
            _scrollViewer = scrollViewer;
            _scrollViewer.ScrollChanged += ScrollViewerScrollChanged;

            // assume we do the regulation of the horizontal scrollbar
            _scrollViewer.SetCurrentValue(ScrollViewer.HorizontalScrollBarVisibilityProperty, ScrollBarVisibility.Hidden);
            _scrollViewer.SetCurrentValue(ScrollViewer.VerticalScrollBarVisibilityProperty, VerticalScrollBarVisibility);
        }

        private void UnregisterEvents(DependencyObject? start)
        {
            if (start is null)
            {
                return;
            }

            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(start); i++)
            {
                var childVisual = VisualTreeHelper.GetChild(start, i) as Visual;
                switch (childVisual)
                {
                    case Thumb thumb:
                        UnregisterThumb(thumb);
                        break;

                    case GridViewColumnHeader columnHeader:
                        columnHeader.SizeChanged -= GridColumnHeaderSizeChanged;
                        break;

                    case ScrollViewer scrollViewer when ReferenceEquals(scrollViewer, _scrollViewer):
                        scrollViewer.ScrollChanged -= ScrollViewerScrollChanged;
                        // so that RegisterEvents picks it up again, once the ListView is loaded again
                        _scrollViewer = null;
                        break;
                }

                UnregisterEvents(childVisual);
            }
        }

        private void UnregisterThumb(Thumb thumb)
        {
            var gridViewColumn = FindParentColumn(thumb);
            if (gridViewColumn is null)
            {
                return;
            }

            if (IsResizeSuppressed(gridViewColumn))
            {
                thumb.SetCurrentValue(UIElement.IsHitTestVisibleProperty, true);
                return;
            }

            thumb.PreviewMouseMove -= ThumbPreviewMouseMove;
            thumb.PreviewMouseLeftButtonDown -= ThumbPreviewMouseLeftButtonDown;
            DependencyPropertyDescriptor.FromProperty(
                GridViewColumn.WidthProperty,
                typeof(GridViewColumn)).RemoveValueChanged(gridViewColumn, GridColumnWidthChanged);
        }

        /// <summary>
        /// proportional, fixed and range fill columns size themselves, so the user can't resize them
        /// </summary>
        private bool IsResizeSuppressed(GridViewColumn? gridViewColumn)
        {
            return ProportionalColumn.IsProportionalColumn(gridViewColumn)
                || FixedColumn.IsFixedColumn(gridViewColumn)
                || IsFillColumn(gridViewColumn);
        }

        private static GridViewColumn? FindParentColumn(DependencyObject? element)
        {
            if (element is null)
            {
                return null;
            }

            while (element != null)
            {
                if (element is GridViewColumnHeader gridViewColumnHeader)
                {
                    return gridViewColumnHeader.Column;
                }
                element = VisualTreeHelper.GetParent(element);
            }

            return null;
        }

        private static GridViewColumnHeader? FindColumnHeader(DependencyObject? start, GridViewColumn? gridViewColumn)
        {
            if (start is null)
            {
                return null;
            }

            if (gridViewColumn is null)
            {
                return null;
            }

            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(start); i++)
            {
                var childVisual = VisualTreeHelper.GetChild(start, i) as Visual;
                if (childVisual is GridViewColumnHeader)
                {
                    if (childVisual is GridViewColumnHeader gridViewHeader)
                    {
                        if (gridViewHeader.Column == gridViewColumn)
                        {
                            return gridViewHeader;
                        }
                    }
                }
                var childGridViewHeader = FindColumnHeader(childVisual, gridViewColumn);
                if (childGridViewHeader != null)
                {
                    return childGridViewHeader;
                }
            }
            return null;
        }

        private void InitColumns()
        {
            if (ListView.View is not GridView gridView)
            {
                return;
            }

            foreach (var gridViewColumn in gridView.Columns)
            {
                InitRangeColumn(gridViewColumn);
            }
        }

        private void InitRangeColumn(GridViewColumn gridViewColumn)
        {
            if (!RangeColumn.IsRangeColumn(gridViewColumn))
            {
                return;
            }

            var minWidth = RangeColumn.GetRangeMinWidth(gridViewColumn);
            var maxWidth = RangeColumn.GetRangeMaxWidth(gridViewColumn);
            if (!minWidth.HasValue && !maxWidth.HasValue)
            {
                return;
            }

            var columnHeader = FindColumnHeader(ListView, gridViewColumn);
            if (columnHeader is null)
            {
                return;
            }

            var actualWidth = columnHeader.ActualWidth;
            var hasActualWidth = !double.IsInfinity(actualWidth);
            if (minWidth.HasValue)
            {
                columnHeader.SetCurrentValue(FrameworkElement.MinWidthProperty, minWidth.Value);
                if (hasActualWidth && actualWidth < columnHeader.MinWidth)
                {
                    gridViewColumn.SetCurrentValue(GridViewColumn.WidthProperty, columnHeader.MinWidth);
                }
            }

            if (maxWidth.HasValue)
            {
                columnHeader.SetCurrentValue(FrameworkElement.MaxWidthProperty, maxWidth.Value);
                if (hasActualWidth && actualWidth > columnHeader.MaxWidth)
                {
                    gridViewColumn.SetCurrentValue(GridViewColumn.WidthProperty, columnHeader.MaxWidth);
                }
            }
        }

        private void ResizeColumns()
        {
            if (ListView.View is not GridView view || view.Columns.Count == 0)
            {
                return;
            }

            var actualWidth = GetAvailableWidth();
            if (double.IsInfinity(actualWidth) || actualWidth <= 0)
            {
                return;
            }

            double resizeableRegionCount = 0;
            double otherColumnsWidth = 0;

            // determine column sizes
            foreach (var gridViewColumn in view.Columns)
            {
                if (ProportionalColumn.IsProportionalColumn(gridViewColumn))
                {
                    resizeableRegionCount += ProportionalColumn.GetProportionalWidth(gridViewColumn) ?? 0;
                }
                else
                {
                    otherColumnsWidth += gridViewColumn.ActualWidth;
                }
            }

            if (resizeableRegionCount <= 0)
            {
                ResizeFillColumn(view, actualWidth, otherColumnsWidth);
                return;
            }

            ResizeProportionalColumns(view, actualWidth - otherColumnsWidth, resizeableRegionCount);
        }

        private double GetAvailableWidth()
        {
            var actualWidth = _scrollViewer?.ViewportWidth ?? double.PositiveInfinity;
            return double.IsInfinity(actualWidth)
                ? ListView.ActualWidth
                : actualWidth;
        }

        private void ResizeFillColumn(GridView view, double actualWidth, double otherColumnsWidth)
        {
            // no proportional columns present: commit the regulation to the scroll viewer
            _scrollViewer?.SetCurrentValue(ScrollViewer.HorizontalScrollBarVisibilityProperty, ScrollBarVisibility.Auto);

            var fillColumn = view.Columns.FirstOrDefault(IsFillColumn);
            if (fillColumn is null)
            {
                return;
            }

            var otherColumnsWithoutFillWidth = otherColumnsWidth - fillColumn.ActualWidth;
            var fillWidth = actualWidth - otherColumnsWithoutFillWidth;
            if (fillWidth <= 0)
            {
                return;
            }

            var minWidth = RangeColumn.GetRangeMinWidth(fillColumn);
            var maxWidth = RangeColumn.GetRangeMaxWidth(fillColumn);
            if (fillWidth < minWidth || fillWidth > maxWidth)
            {
                return;
            }

            _scrollViewer?.SetCurrentValue(ScrollViewer.HorizontalScrollBarVisibilityProperty, ScrollBarVisibility.Hidden);
            fillColumn.SetCurrentValue(GridViewColumn.WidthProperty, fillWidth);
        }

        private static void ResizeProportionalColumns(GridView view, double resizeableColumnsWidth, double resizeableRegionCount)
        {
            if (resizeableColumnsWidth <= 0)
            {
                return;
            }

            var resizeableRegionWidth = resizeableColumnsWidth / resizeableRegionCount;
            foreach (var gridViewColumn in view.Columns)
            {
                if (!ProportionalColumn.IsProportionalColumn(gridViewColumn))
                {
                    continue;
                }

                var proportionalWidth = ProportionalColumn.GetProportionalWidth(gridViewColumn);
                if (proportionalWidth.HasValue)
                {
                    gridViewColumn.SetCurrentValue(GridViewColumn.WidthProperty, proportionalWidth.Value * resizeableRegionWidth);
                }
            }
        }

        // returns the delta
        private static double SetRangeColumnToBounds(GridViewColumn gridViewColumn)
        {
            var startWidth = gridViewColumn.Width;

            var minWidth = RangeColumn.GetRangeMinWidth(gridViewColumn);
            var maxWidth = RangeColumn.GetRangeMaxWidth(gridViewColumn);

            if (minWidth.HasValue && maxWidth.HasValue && (minWidth > maxWidth))
            {
                return 0; // invalid case
            }

            if (minWidth.HasValue && gridViewColumn.Width < minWidth.Value)
            {
                gridViewColumn.SetCurrentValue(GridViewColumn.WidthProperty, minWidth.Value);
            }
            else if (maxWidth.HasValue && gridViewColumn.Width > maxWidth.Value)
            {
                gridViewColumn.SetCurrentValue(GridViewColumn.WidthProperty, maxWidth.Value);
            }

            return gridViewColumn.Width - startWidth;
        }

        private bool IsFillColumn(GridViewColumn? gridViewColumn)
        {
            if (gridViewColumn is null)
            {
                return false;
            }

            var view = ListView.View as GridView;
            if (view is null || view.Columns.Count == 0)
            {
                return false;
            }

            var isFillColumn = RangeColumn.GetRangeIsFillColumn(gridViewColumn);
            return isFillColumn == true;
        }

        private void DoResizeColumns()
        {
            if (_resizing)
            {
                return;
            }

            _resizing = true;
            try
            {
                ResizeColumns();
            }
            finally
            {
                _resizing = false;
            }
        }

        private void ListViewLoaded(object sender, RoutedEventArgs e)
        {
            RegisterEvents(ListView);
            InitColumns();
            DoResizeColumns();
            _loaded = true;
        }

        private void ListViewUnloaded(object sender, RoutedEventArgs e)
        {
            if (!_loaded)
            {
                return;
            }
            UnregisterEvents(ListView);
            _loaded = false;
        }

        private void ThumbPreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (sender is not Thumb thumb)
            {
                return;
            }
            var gridViewColumn = FindParentColumn(thumb);
            if (gridViewColumn is null)
            {
                return;
            }

            if (IsResizeSuppressed(gridViewColumn))
            {
                thumb.SetCurrentValue(FrameworkElement.CursorProperty, null);
                return;
            }

            if (thumb.IsMouseCaptured && RangeColumn.IsRangeColumn(gridViewColumn))
            {
                UpdateRangeResizeCursor(thumb, gridViewColumn);
            }
        }

        private void UpdateRangeResizeCursor(Thumb thumb, GridViewColumn gridViewColumn)
        {
            var minWidth = RangeColumn.GetRangeMinWidth(gridViewColumn);
            var maxWidth = RangeColumn.GetRangeMaxWidth(gridViewColumn);

            if (minWidth > maxWidth)
            {
                return; // invalid case
            }

            _resizeCursor ??= thumb.Cursor; // save the resize cursor

            var isAtBounds = gridViewColumn.Width <= minWidth || gridViewColumn.Width >= maxWidth;
            thumb.SetCurrentValue(FrameworkElement.CursorProperty, isAtBounds ? Cursors.No : _resizeCursor);
        }

        private void ThumbPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is not Thumb thumb)
            {
                return;
            }

            var gridViewColumn = FindParentColumn(thumb);
            if (IsResizeSuppressed(gridViewColumn))
            {
                e.Handled = true;
            }
        }

        private void GridColumnWidthChanged(object? sender, EventArgs? e)
        {
            if (!_loaded)
            {
                return;
            }
            if (sender is GridViewColumn gridViewColumn)
            {
                // suppress column resizing for proportional and fixed columns
                if (ProportionalColumn.IsProportionalColumn(gridViewColumn) || FixedColumn.IsFixedColumn(gridViewColumn))
                {
                    return;
                }

                // ensure range column within the bounds
                if (RangeColumn.IsRangeColumn(gridViewColumn))
                {
                    // special case: auto column width - maybe conflicts with min/max range
                    if (gridViewColumn.Width.Equals(double.NaN))
                    {
                        _autoSizedColumn = gridViewColumn;
                        return; // handled by the change header size event
                    }

                    // ensure column bounds
                    if (Math.Abs(SetRangeColumnToBounds(gridViewColumn) - 0) > ZeroWidthRange)
                    {
                        return;
                    }
                }
            }

            DoResizeColumns();
        }

        private void GridColumnHeaderSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_autoSizedColumn is null)
            {
                return;
            }

            if (sender is GridViewColumnHeader gridViewColumnHeader && gridViewColumnHeader.Column == _autoSizedColumn)
            {
                if (gridViewColumnHeader.Width.Equals(double.NaN))
                {
                    // sync column with
                    gridViewColumnHeader.Column.SetCurrentValue(GridViewColumn.WidthProperty, gridViewColumnHeader.ActualWidth);
                    DoResizeColumns();
                }

                _autoSizedColumn = null;
            }
        }

        private void ScrollViewerScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (_loaded && Math.Abs(e.ViewportWidthChange - 0) > ZeroWidthRange)
            {
                DoResizeColumns();
            }
        }

        private static void OnEnabledChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            if (dependencyObject is ListView listView)
            {
                var enabled = (bool)e.NewValue;
                if (enabled)
                {
                    new ListViewLayoutManager(listView);
                }
            }
        }
    }
}
