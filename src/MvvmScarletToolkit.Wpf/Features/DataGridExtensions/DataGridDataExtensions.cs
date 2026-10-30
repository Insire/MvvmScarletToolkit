using System.Data;
using System.Windows.Controls;

namespace MvvmScarletToolkit
{
    /// <summary>
    /// Read the content of a <see cref="DataGrid"/>, see <see cref="IDataGridDataSource"/>
    /// </summary>
    public static class DataGridDataExtensions
    {
        public static IDataGridDataSource AsDataSource(this DataGrid dataGrid)
        {
            return new DataGridDataSource(dataGrid);
        }

        /// <inheritdoc cref="IDataGridDataSource.ToDataTable(CancellationToken)"/>
        public static Task<DataTable> ToDataTable(this DataGrid dataGrid, CancellationToken token = default)
        {
            return dataGrid.AsDataSource().ToDataTable(token);
        }

        /// <inheritdoc cref="IDataGridDataSource.ToDataTable{TItem}(CancellationToken)"/>
        public static Task<DataTable> ToDataTable<TItem>(this DataGrid dataGrid, CancellationToken token = default)
        {
            return dataGrid.AsDataSource().ToDataTable<TItem>(token);
        }

        /// <inheritdoc cref="IDataGridDataSource.ToEnumerable{TResult}(Func{DataRow, TResult}, CancellationToken)"/>
        public static Task<IEnumerable<TResult>> ToEnumerable<TResult>(this DataGrid dataGrid, Func<DataRow, TResult> mapper, CancellationToken token = default)
        {
            return dataGrid.AsDataSource().ToEnumerable(mapper, token);
        }

        /// <inheritdoc cref="IDataGridDataSource.GetItems{TItem}(CancellationToken)"/>
        public static Task<DataGridItems<TItem>> GetItems<TItem>(this DataGrid dataGrid, CancellationToken token = default)
        {
            return dataGrid.AsDataSource().GetItems<TItem>(token);
        }
    }
}
