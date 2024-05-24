using System;
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
                if (childVisual is Thumb)
                {
                    var gridViewColumn = ListViewLayoutManager.FindParentColumn(childVisual);
                    if (gridViewColumn != null)
                    {
                        if (childVisual is Thumb thumb)
                        {
                            if (ProportionalColumn.IsProportionalColumn(gridViewColumn) || FixedColumn.IsFixedColumn(gridViewColumn) || IsFillColumn(gridViewColumn))
                            {
                                thumb.SetCurrentValue(UIElement.IsHitTestVisibleProperty, false);
                            }
                            else
                            {
                                thumb.PreviewMouseMove += ThumbPreviewMouseMove;
                                thumb.PreviewMouseLeftButtonDown += ThumbPreviewMouseLeftButtonDown;
                                DependencyPropertyDescriptor.FromProperty(
                                    GridViewColumn.WidthProperty,
                                    typeof(GridViewColumn)).AddValueChanged(gridViewColumn, GridColumnWidthChanged);
                            }
                        }
                    }
                }
                else if (childVisual is GridViewColumnHeader columnHeader)
                {
                    columnHeader.SizeChanged += GridColumnHeaderSizeChanged;
                }
                else if (_scrollViewer is null && childVisual is ScrollViewer scrollViewer)
                {
                    _scrollViewer = scrollViewer;
                    _scrollViewer.ScrollChanged += ScrollViewerScrollChanged;

                    // assume we do the regulation of the horizontal scrollbar
                    _scrollViewer.SetCurrentValue(ScrollViewer.HorizontalScrollBarVisibilityProperty, ScrollBarVisibility.Hidden);
                    _scrollViewer.SetCurrentValue(ScrollViewer.VerticalScrollBarVisibilityProperty, VerticalScrollBarVisibility);
                }

                RegisterEvents(childVisual);
            }
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
                if (childVisual is Thumb)
                {
                    var gridViewColumn = ListViewLayoutManager.FindParentColumn(childVisual);
                    if (gridViewColumn != null)
                    {
                        if (childVisual is Thumb thumb)
                        {
                            if (ProportionalColumn.IsProportionalColumn(gridViewColumn) || FixedColumn.IsFixedColumn(gridViewColumn) || IsFillColumn(gridViewColumn))
                            {
                                thumb.SetCurrentValue(UIElement.IsHitTestVisibleProperty, true);
                            }
                            else
                            {
                                thumb.PreviewMouseMove -= ThumbPreviewMouseMove;
                                thumb.PreviewMouseLeftButtonDown -= ThumbPreviewMouseLeftButtonDown;
                                DependencyPropertyDescriptor.FromProperty(
                                    GridViewColumn.WidthProperty,
                                    typeof(GridViewColumn)).RemoveValueChanged(gridViewColumn, GridColumnWidthChanged);
                            }
                        }
                    }
                }
                else if (childVisual is GridViewColumnHeader columnHeader)
                {
                    columnHeader.SizeChanged -= GridColumnHeaderSizeChanged;
                }
                else if (_scrollViewer is null && childVisual is ScrollViewer scrollViewer)
                {
                    _scrollViewer = scrollViewer;
                    _scrollViewer.ScrollChanged -= ScrollViewerScrollChanged;
                }

                UnregisterEvents(childVisual);
            }
        }

        private static GridViewColumn? FindParentColumn(DependencyObject element)
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
                var childGridViewHeader = ListViewLayoutManager.FindColumnHeader(childVisual, gridViewColumn);
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
                if (!RangeColumn.IsRangeColumn(gridViewColumn))
                {
                    continue;
                }

                var minWidth = RangeColumn.GetRangeMinWidth(gridViewColumn);
                var maxWidth = RangeColumn.GetRangeMaxWidth(gridViewColumn);
                if (!minWidth.HasValue && !maxWidth.HasValue)
                {
                    continue;
                }

                var columnHeader = ListViewLayoutManager.FindColumnHeader(ListView, gridViewColumn);
                if (columnHeader is null)
                {
                    continue;
                }

                var actualWidth = columnHeader.ActualWidth;
                if (minWidth.HasValue)
                {
                    columnHeader.SetCurrentValue(FrameworkElement.MinWidthProperty, minWidth.Value);
                    if (!double.IsInfinity(actualWidth) && actualWidth < columnHeader.MinWidth)
                    {
                        gridViewColumn.SetCurrentValue(GridViewColumn.WidthProperty, columnHeader.MinWidth);
                    }
                }
                if (maxWidth.HasValue)
                {
                    columnHeader.SetCurrentValue(FrameworkElement.MaxWidthProperty, maxWidth.Value);
                    if (!double.IsInfinity(actualWidth) && actualWidth > columnHeader.MaxWidth)
                    {
                        gridViewColumn.SetCurrentValue(GridViewColumn.WidthProperty, columnHeader.MaxWidth);
                    }
                }
            }
        }

        private void ResizeColumns()
        {
            var view = ListView.View as GridView;
            if (view is null || view.Columns.Count == 0)
            {
                return;
            }

            var actualWidth = double.PositiveInfinity;
            if (_scrollViewer != null)
            {
                actualWidth = _scrollViewer.ViewportWidth;
            }
            if (double.IsInfinity(actualWidth))
            {
                actualWidth = ListView.ActualWidth;
            }
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
                    var proportionalWidth = ProportionalColumn.GetProportionalWidth(gridViewColumn);
                    if (proportionalWidth != null)
                    {
                        resizeableRegionCount += proportionalWidth.Value;
                    }
                }
                else
                {
                    otherColumnsWidth += gridViewColumn.ActualWidth;
                }
            }

            if (resizeableRegionCount <= 0)
            {
                // no proportional columns present: commit the regulation to the scroll viewer
                _scrollViewer?.SetCurrentValue(ScrollViewer.HorizontalScrollBarVisibilityProperty, ScrollBarVisibility.Auto);

                // search the first fill column
                GridViewColumn? fillColumn = null;
                for (var i = 0; i < view.Columns.Count; i++)
                {
                    var gridViewColumn = view.Columns[i];
                    if (IsFillColumn(gridViewColumn))
                    {
                        fillColumn = gridViewColumn;
                        break;
                    }
                }

                if (fillColumn != null)
                {
                    var otherColumnsWithoutFillWidth = otherColumnsWidth - fillColumn.ActualWidth;
                    var fillWidth = actualWidth - otherColumnsWithoutFillWidth;
                    if (fillWidth > 0)
                    {
                        var minWidth = RangeColumn.GetRangeMinWidth(fillColumn);
                        var maxWidth = RangeColumn.GetRangeMaxWidth(fillColumn);

                        var setWidth = !(minWidth.HasValue && fillWidth < minWidth.Value);
                        if (maxWidth.HasValue && fillWidth > maxWidth.Value)
                        {
                            setWidth = false;
                        }
                        if (setWidth)
                        {
                            _scrollViewer?.SetCurrentValue(ScrollViewer.HorizontalScrollBarVisibilityProperty, ScrollBarVisibility.Hidden);
                            fillColumn.SetCurrentValue(GridViewColumn.WidthProperty, fillWidth);
                        }
                    }
                }
                return;
            }

            var resizeableColumnsWidth = actualWidth - otherColumnsWidth;
            if (resizeableColumnsWidth <= 0)
            {
                return;
            }

            // resize columns
            var resizeableRegionWidth = resizeableColumnsWidth / resizeableRegionCount;
            foreach (var gridViewColumn in view.Columns)
            {
                if (ProportionalColumn.IsProportionalColumn(gridViewColumn))
                {
                    var proportionalWidth = ProportionalColumn.GetProportionalWidth(gridViewColumn);
                    if (proportionalWidth != null)
                    {
                        gridViewColumn.SetCurrentValue(GridViewColumn.WidthProperty, proportionalWidth.Value * resizeableRegionWidth);
                    }
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
            var gridViewColumn = ListViewLayoutManager.FindParentColumn(thumb);
            if (gridViewColumn is null)
            {
                return;
            }

            // suppress column resizing for proportional, fixed and range fill columns
            if (ProportionalColumn.IsProportionalColumn(gridViewColumn) || FixedColumn.IsFixedColumn(gridViewColumn) || IsFillColumn(gridViewColumn))
            {
                thumb.SetCurrentValue(FrameworkElement.CursorProperty, null);
                return;
            }

            // check range column bounds
            if (thumb.IsMouseCaptured && RangeColumn.IsRangeColumn(gridViewColumn))
            {
                var minWidth = RangeColumn.GetRangeMinWidth(gridViewColumn);
                var maxWidth = RangeColumn.GetRangeMaxWidth(gridViewColumn);

                if (minWidth.HasValue && maxWidth.HasValue && (minWidth > maxWidth))
                {
                    return; // invalid case
                }

                _resizeCursor ??= thumb.Cursor; // save the resize cursor

                if (minWidth.HasValue && gridViewColumn.Width <= minWidth.Value)
                {
                    thumb.SetCurrentValue(FrameworkElement.CursorProperty, Cursors.No);
                }
                else if (maxWidth.HasValue && gridViewColumn.Width >= maxWidth.Value)
                {
                    thumb.SetCurrentValue(FrameworkElement.CursorProperty, Cursors.No);
                }
                else
                {
                    thumb.SetCurrentValue(FrameworkElement.CursorProperty, _resizeCursor); // between valid min/max
                }
            }
        }

        private void ThumbPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is not Thumb thumb)
            {
                return;
            }

            var gridViewColumn = ListViewLayoutManager.FindParentColumn(thumb);

            // suppress column resizing for proportional, fixed and range fill columns
            if (ProportionalColumn.IsProportionalColumn(gridViewColumn) || FixedColumn.IsFixedColumn(gridViewColumn) || IsFillColumn(gridViewColumn))
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
                    if (Math.Abs(ListViewLayoutManager.SetRangeColumnToBounds(gridViewColumn) - 0) > ZeroWidthRange)
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
