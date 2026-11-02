using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace MvvmScarletToolkit
{
    public static class DataGridLayoutExtensions
    {
        /// <summary>
        /// Reads the current state of the columns of <paramref name="dataGrid"/>
        /// </summary>
        public static DataGridLayout CaptureLayout(this DataGrid dataGrid)
        {
            ArgumentNullException.ThrowIfNull(dataGrid);

            var sorting = dataGrid.Items.SortDescriptions.ToList();
            var columns = new List<DataGridColumnLayout>(dataGrid.Columns.Count);

            for (var i = 0; i < dataGrid.Columns.Count; i++)
            {
                var column = dataGrid.Columns[i];
                var sortIndex = sorting.FindIndex(p => p.PropertyName == column.SortMemberPath);

                columns.Add(new DataGridColumnLayout
                {
                    Key = column.GetKey(i),
                    IsVisible = column.Visibility == Visibility.Visible,
                    DisplayIndex = column.DisplayIndex,
                    Width = column.Width.Value,
                    WidthType = column.Width.UnitType,
                    SortDirection = sortIndex >= 0 ? sorting[sortIndex].Direction : null,
                    SortIndex = sortIndex >= 0 ? sortIndex : null,
                    SummaryFunction = DataGridSummaryBar.GetFunction(column),
                });
            }

            return new DataGridLayout { Columns = columns };
        }

        /// <summary>
        /// Applies <paramref name="layout"/> to the columns of <paramref name="dataGrid"/>. Columns unknown to the layout are left as they are, entries without a column are ignored.
        /// </summary>
        public static void ApplyLayout(this DataGrid dataGrid, DataGridLayout layout)
        {
            ArgumentNullException.ThrowIfNull(dataGrid);
            ArgumentNullException.ThrowIfNull(layout);

            var columns = new Dictionary<string, DataGridColumn>();
            for (var i = 0; i < dataGrid.Columns.Count; i++)
            {
                columns.TryAdd(dataGrid.Columns[i].GetKey(i), dataGrid.Columns[i]);
            }

            var known = layout.Columns.Where(p => columns.ContainsKey(p.Key)).ToArray();

            foreach (var entry in known)
            {
                var column = columns[entry.Key];
                column.Visibility = entry.IsVisible ? Visibility.Visible : Visibility.Collapsed;
                column.Width = new DataGridLength(entry.Width, entry.WidthType);

                if (entry.SummaryFunction is { } function)
                {
                    DataGridSummaryBar.SetFunction(column, function);
                }
            }

            // ascending, so that an entry isn't moved again by the ones after it
            foreach (var entry in known.OrderBy(p => p.DisplayIndex))
            {
                if (entry.DisplayIndex >= 0 && entry.DisplayIndex < dataGrid.Columns.Count)
                {
                    columns[entry.Key].DisplayIndex = entry.DisplayIndex;
                }
            }

            ApplySorting(dataGrid, columns, known);
        }

        private static void ApplySorting(DataGrid dataGrid, Dictionary<string, DataGridColumn> columns, DataGridColumnLayout[] layout)
        {
            var sortDescriptions = dataGrid.Items.SortDescriptions;

            using (dataGrid.Items.DeferRefresh())
            {
                sortDescriptions.Clear();

                foreach (var column in dataGrid.Columns)
                {
                    column.SortDirection = null;
                }

                foreach (var entry in layout.Where(p => p.SortDirection.HasValue).OrderBy(p => p.SortIndex))
                {
                    var column = columns[entry.Key];
                    if (string.IsNullOrEmpty(column.SortMemberPath))
                    {
                        continue;
                    }

                    column.SortDirection = entry.SortDirection;
                    sortDescriptions.Add(new SortDescription(column.SortMemberPath, entry.SortDirection!.Value));
                }
            }
        }
    }
}
