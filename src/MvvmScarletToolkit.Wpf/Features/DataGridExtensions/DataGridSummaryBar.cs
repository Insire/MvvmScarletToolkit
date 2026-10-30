using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;

namespace MvvmScarletToolkit
{
    /// <summary>
    /// A row of summaries below a <see cref="DataGrid"/>, that follows its column widths and horizontal scrolling. The values are calculated over the filtered rows in the background.
    /// Columns start with <see cref="FunctionProperty"/>. Users can change it by right-clicking a cell, unless <see cref="CanChangeFunction"/> is disabled. Frozen columns are not supported.
    /// </summary>
    /// <remarks>
    /// required namespaces:
    /// <list type="bullet">
    /// <item>
    /// <description>xmlns:mvvm="http://SoftThorn.MvvmScarletToolkit.com/winfx/xaml/shared"</description>
    /// </item>
    /// </list>
    /// </remarks>
    // usage:
    // <DataGridTextColumn mvvm:DataGridSummaryBar.Function="Sum" Binding="{Binding Price}" />
    // <mvvm:DataGridSummaryBar DataGrid="{Binding ElementName=Grid}" />
    public class DataGridSummaryBar : Control
    {
        private static readonly DependencyPropertyDescriptor _actualWidthDescriptor = DependencyPropertyDescriptor.FromProperty(DataGridColumn.ActualWidthProperty, typeof(DataGridColumn));
        private static readonly DependencyPropertyDescriptor _visibilityDescriptor = DependencyPropertyDescriptor.FromProperty(DataGridColumn.VisibilityProperty, typeof(DataGridColumn));

        private readonly List<DataGridColumn> _trackedColumns = [];
        private readonly List<DataGridColumn> _columns = [];

        private bool _isCalculationPending;
        private double _horizontalOffset;

        public ObservableCollection<DataGridSummaryCell> Cells { get; } = [];

        /// <summary>
        /// The function of a column
        /// </summary>
        public static readonly DependencyProperty FunctionProperty = DependencyProperty.RegisterAttached(
            "Function",
            typeof(DataGridSummaryFunction),
            typeof(DataGridSummaryBar),
            new PropertyMetadata(DataGridSummaryFunction.None));

        public static void SetFunction(DependencyObject element, DataGridSummaryFunction value)
        {
            element.SetValue(FunctionProperty, value);
        }

        [AttachedPropertyBrowsableForType(typeof(DataGridColumn))]
        public static DataGridSummaryFunction GetFunction(DependencyObject element)
        {
            return (DataGridSummaryFunction)element.GetValue(FunctionProperty);
        }

        private static readonly DependencyPropertyDescriptor _functionDescriptor = DependencyPropertyDescriptor.FromProperty(FunctionProperty, typeof(DataGridColumn));

        /// <summary>
        /// The functions a user can choose from for a column, like "Sum, Maximum". By default this depends on the type of the property. None prevents changing the function of this column.
        /// </summary>
        public static readonly DependencyProperty FunctionsProperty = DependencyProperty.RegisterAttached(
            "Functions",
            typeof(DataGridSummaryFunction?),
            typeof(DataGridSummaryBar),
            new PropertyMetadata(null));

        public static void SetFunctions(DependencyObject element, DataGridSummaryFunction? value)
        {
            element.SetValue(FunctionsProperty, value);
        }

        [AttachedPropertyBrowsableForType(typeof(DataGridColumn))]
        public static DataGridSummaryFunction? GetFunctions(DependencyObject element)
        {
            return (DataGridSummaryFunction?)element.GetValue(FunctionsProperty);
        }

        /// <summary>
        /// Whether users can change the function of a column through the context menu of its cell
        /// </summary>
        public bool CanChangeFunction
        {
            get { return (bool)GetValue(CanChangeFunctionProperty); }
            set { SetValue(CanChangeFunctionProperty, value); }
        }

        public static readonly DependencyProperty CanChangeFunctionProperty = DependencyProperty.Register(
            nameof(CanChangeFunction),
            typeof(bool),
            typeof(DataGridSummaryBar),
            new PropertyMetadata(true, OnCanChangeFunctionChanged));

        private static void OnCanChangeFunctionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DataGridSummaryBar)d).UpdateCells();
        }

        /// <summary>
        /// A format string for the value of a column, like "N2"
        /// </summary>
        public static readonly DependencyProperty FormatProperty = DependencyProperty.RegisterAttached(
            "Format",
            typeof(string),
            typeof(DataGridSummaryBar),
            new PropertyMetadata(null));

        public static void SetFormat(DependencyObject element, string? value)
        {
            element.SetValue(FormatProperty, value);
        }

        [AttachedPropertyBrowsableForType(typeof(DataGridColumn))]
        public static string? GetFormat(DependencyObject element)
        {
            return (string?)element.GetValue(FormatProperty);
        }

        public DataGrid? DataGrid
        {
            get { return (DataGrid?)GetValue(DataGridProperty); }
            set { SetValue(DataGridProperty, value); }
        }

        public static readonly DependencyProperty DataGridProperty = DependencyProperty.Register(
            nameof(DataGrid),
            typeof(DataGrid),
            typeof(DataGridSummaryBar),
            new PropertyMetadata(null, OnDataGridChanged));

        private static readonly DependencyPropertyKey _cellsMarginPropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(CellsMargin),
            typeof(Thickness),
            typeof(DataGridSummaryBar),
            new PropertyMetadata(default(Thickness)));

        public static readonly DependencyProperty CellsMarginProperty = _cellsMarginPropertyKey.DependencyProperty;

        /// <summary>
        /// Moves the cells to the position of the columns. Its left side is negative while the grid is scrolled.
        /// </summary>
        public Thickness CellsMargin
        {
            get { return (Thickness)GetValue(CellsMarginProperty); }
            private set { SetValue(_cellsMarginPropertyKey, value); }
        }

        private static void OnDataGridChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DataGridSummaryBar bar)
            {
                return;
            }

            if (e.OldValue is DataGrid oldGrid)
            {
                bar.Disconnect(oldGrid);
            }

            if (e.NewValue is DataGrid newGrid)
            {
                bar.Connect(newGrid);
            }
        }

        private void Connect(DataGrid dataGrid)
        {
            dataGrid.Columns.CollectionChanged += OnColumnsChanged;
            dataGrid.ColumnDisplayIndexChanged += OnColumnDisplayIndexChanged;
            dataGrid.CellEditEnding += OnCellEditEnding;
            dataGrid.Loaded += OnDataGridLoaded;
            dataGrid.AddHandler(ScrollViewer.ScrollChangedEvent, new ScrollChangedEventHandler(OnScrollChanged));
            ((INotifyCollectionChanged)dataGrid.Items).CollectionChanged += OnItemsChanged;

            UpdateCells();
        }

        private void Disconnect(DataGrid dataGrid)
        {
            dataGrid.Columns.CollectionChanged -= OnColumnsChanged;
            dataGrid.ColumnDisplayIndexChanged -= OnColumnDisplayIndexChanged;
            dataGrid.CellEditEnding -= OnCellEditEnding;
            dataGrid.Loaded -= OnDataGridLoaded;
            dataGrid.RemoveHandler(ScrollViewer.ScrollChangedEvent, new ScrollChangedEventHandler(OnScrollChanged));
            ((INotifyCollectionChanged)dataGrid.Items).CollectionChanged -= OnItemsChanged;

            Untrack();
            _columns.Clear();
            Cells.Clear();
        }

        private void Untrack()
        {
            // the descriptors keep strong references to their handlers
            foreach (var column in _trackedColumns)
            {
                _actualWidthDescriptor.RemoveValueChanged(column, OnColumnLayoutChanged);
                _visibilityDescriptor.RemoveValueChanged(column, OnColumnVisibilityChanged);
                _functionDescriptor.RemoveValueChanged(column, OnColumnFunctionChanged);
            }

            _trackedColumns.Clear();
        }

        private void OnColumnsChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateCells();
        }

        private void OnColumnDisplayIndexChanged(object? sender, DataGridColumnEventArgs e)
        {
            UpdateCells();
        }

        private void OnDataGridLoaded(object sender, RoutedEventArgs e)
        {
            UpdateCells();
        }

        private void OnColumnVisibilityChanged(object? sender, EventArgs e)
        {
            UpdateCells();
        }

        private async void OnColumnFunctionChanged(object? sender, EventArgs e)
        {
            // a hidden column has no cell
            var index = sender is DataGridColumn column ? _columns.IndexOf(column) : -1;
            if (index < 0)
            {
                return;
            }

            var cell = Cells[index];
            cell.Description = GetDescription(_columns[index], index);

            if (GetFunction(_columns[index]) == DataGridSummaryFunction.None)
            {
                // also drops the result of a calculation that is still running
                cell.Version++;
                cell.Text = null;
                return;
            }

            await Calculate([index]);
        }

        private void OnColumnLayoutChanged(object? sender, EventArgs e)
        {
            UpdateWidths();
        }

        private void OnItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            ScheduleCalculation();
        }

        private void OnCellEditEnding(object? sender, DataGridCellEditEndingEventArgs e)
        {
            ScheduleCalculation();
        }

        private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            // templates of cells may contain scrollviewers of their own
            if (e.OriginalSource is ScrollViewer { TemplatedParent: System.Windows.Controls.DataGrid })
            {
                _horizontalOffset = e.HorizontalOffset;
                UpdateOffset();
            }
        }

        private void UpdateOffset()
        {
            CellsMargin = new Thickness((DataGrid?.RowHeaderActualWidth ?? 0) - _horizontalOffset, 0, 0, 0);
        }

        private void UpdateCells()
        {
            Untrack();
            _columns.Clear();
            Cells.Clear();

            if (DataGrid is null)
            {
                return;
            }

            foreach (var column in DataGrid.Columns)
            {
                _actualWidthDescriptor.AddValueChanged(column, OnColumnLayoutChanged);
                _visibilityDescriptor.AddValueChanged(column, OnColumnVisibilityChanged);
                _functionDescriptor.AddValueChanged(column, OnColumnFunctionChanged);
                _trackedColumns.Add(column);
            }

            foreach (var column in DataGrid.Columns.Where(p => p.Visibility == Visibility.Visible).OrderBy(p => p.DisplayIndex))
            {
                Cells.Add(new DataGridSummaryCell { Width = column.ActualWidth, Description = GetDescription(column, _columns.Count) });
                _columns.Add(column);
            }

            ScheduleCalculation();
        }

        private void UpdateWidths()
        {
            UpdateOffset();

            for (var i = 0; i < _columns.Count; i++)
            {
                Cells[i].Width = _columns[i].ActualWidth;
            }
        }

        private void ScheduleCalculation()
        {
            if (_isCalculationPending)
            {
                return;
            }

            // a refresh of the view raises a lot of changes
            _isCalculationPending = true;
            Dispatcher.BeginInvoke(Calculate, DispatcherPriority.Background);
        }

        /// <summary>
        /// Calculates all values again. Changes of a row are only noticed automatically, when they are made by editing a cell.
        /// </summary>
        public void Refresh()
        {
            ScheduleCalculation();
        }

        private async void Calculate()
        {
            _isCalculationPending = false;

            await Calculate(Enumerable.Range(0, _columns.Count));
        }

        private async Task Calculate(IEnumerable<int> indexes)
        {
            if (DataGrid is null)
            {
                return;
            }

            var work = indexes
                .Select(index => (Cell: Cells[index], Path: _columns[index].GetPropertyPath(), Function: GetFunction(_columns[index]), Format: GetFormat(_columns[index])))
                .Where(p => p.Path is not null && p.Function != DataGridSummaryFunction.None)
                .Select(p => (p.Cell, p.Path, p.Function, p.Format, Version: ++p.Cell.Version))
                .ToArray();

            if (work.Length == 0)
            {
                return;
            }

            var items = DataGrid.Items.Cast<object>().Where(p => !ReferenceEquals(p, CollectionView.NewItemPlaceholder)).ToArray();

            var results = await Task.Run(() => work.Select(p => Format(DataGridSummaryCalculator.Calculate(items, p.Path!, p.Function), p.Format)).ToArray());

            for (var i = 0; i < work.Length; i++)
            {
                // a newer calculation of this cell may have started in the meantime
                if (work[i].Version == work[i].Cell.Version)
                {
                    work[i].Cell.Text = results[i];
                }
            }
        }

        private string? GetDescription(DataGridColumn column, int index)
        {
            var function = GetFunction(column);
            if (function == DataGridSummaryFunction.None || column.GetPropertyPath() is null)
            {
                return CanChange(column) ? "Right-click to add a summary" : null;
            }

            // counts rows, so it doesn't depend on the column
            var description = function == DataGridSummaryFunction.Count
                ? "Number of rows"
                : $"{function} of {column.GetCaption(index)}";

            return CanChange(column)
                ? $"{description}{Environment.NewLine}Right-click to change"
                : description;
        }

        private bool CanChange(DataGridColumn column)
        {
            return CanChangeFunction
                && column.GetPropertyPath() is not null
                && GetFunctions(column) != DataGridSummaryFunction.None;
        }

        private DataGridSummaryFunction GetAvailableFunctions(DataGridColumn column)
        {
            return GetFunctions(column) ?? DataGridSummaryCalculator.GetFunctions(GetPropertyType(column.GetPropertyPath()!));
        }

        private Type? GetPropertyType(string propertyPath)
        {
            var itemType = GetItemType();

            return itemType is null ? null : PropertyAccessor.Get(itemType, propertyPath)?.PropertyType;
        }

        private Type? GetItemType()
        {
            var item = DataGrid?.Items.Cast<object>().FirstOrDefault(p => !ReferenceEquals(p, CollectionView.NewItemPlaceholder));
            if (item is not null)
            {
                return item.GetType();
            }

            // there are no rows to look at, for example while all of them are filtered out
            return DataGrid?.ItemsSource?.GetType().GetInterfaces()
                .FirstOrDefault(p => p.IsGenericType && p.GetGenericTypeDefinition() == typeof(IEnumerable<>))?
                .GetGenericArguments()[0];
        }

        protected override void OnContextMenuOpening(ContextMenuEventArgs e)
        {
            base.OnContextMenuOpening(e);

            if ((e.OriginalSource as FrameworkElement)?.DataContext is not DataGridSummaryCell cell)
            {
                return;
            }

            var menu = CreateMenu(cell);
            if (menu is null)
            {
                return;
            }

            e.Handled = true;

            menu.PlacementTarget = this;
            menu.IsOpen = true;
        }

        /// <returns>null when the function of the column can't be changed</returns>
        internal ContextMenu? CreateMenu(DataGridSummaryCell cell)
        {
            var index = Cells.IndexOf(cell);
            if (index < 0 || !CanChange(_columns[index]))
            {
                return null;
            }

            var column = _columns[index];
            var current = GetFunction(column);
            var available = GetAvailableFunctions(column);
            var menu = new ContextMenu();

            foreach (var function in Enum.GetValues<DataGridSummaryFunction>())
            {
                // a function set in xaml is kept, even when it is not one of the suggested ones
                if (function != DataGridSummaryFunction.None && function != current && !available.HasFlag(function))
                {
                    continue;
                }

                var item = new MenuItem
                {
                    Header = function.ToString(),
                    IsCheckable = true,
                    IsChecked = function == current,
                };

                item.Click += (_, _) => SetFunction(column, function);

                menu.Items.Add(item);
            }

            return menu;
        }

        private static string? Format(object? value, string? format)
        {
            return format is not null && value is IFormattable formattable
                ? formattable.ToString(format, CultureInfo.CurrentCulture)
                : value?.ToString();
        }
    }
}
