using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit
{
    /// <summary>
    /// <para>Provides a snapshot of a data grid: its visible columns in display order, and its rows after filtering in the order shown on screen.</para>
    /// <para>Snapshots are flat. Grouped rows keep their order, rows in collapsed groups are included. A paged grid only provides its current page.</para>
    /// <para>All methods can be called from any thread. Rows are read after they were captured, so an edit during a snapshot may or may not be included.</para>
    /// </summary>
    public interface IDataGridDataSource
    {
        /// <summary>
        /// Reads every value through the bindings of the columns. That requires the UI thread, so this is the slowest way of reading a grid.
        /// </summary>
        Task<DataTable> ToDataTable(CancellationToken token);

        /// <summary>
        /// Reads values directly from the properties of <typeparamref name="TItem"/> on a background thread. Columns are typed by those properties.
        /// Columns that don't map to a property are read through their bindings.
        /// </summary>
        /// <typeparam name="TItem">the type of the rows. It decides how values are read, not which columns are included.</typeparam>
        Task<DataTable> ToDataTable<TItem>(CancellationToken token);

        /// <summary>
        /// Maps each row of <see cref="ToDataTable(CancellationToken)"/> to <typeparamref name="TResult"/>, while the result is being enumerated
        /// </summary>
        Task<IEnumerable<TResult>> ToEnumerable<TResult>(Func<DataRow, TResult> mapper, CancellationToken token);

        /// <summary>
        /// Provides the rows themselves without copying any value. This is the way to go for very large grids.
        /// </summary>
        Task<DataGridItems<TItem>> GetItems<TItem>(CancellationToken token);
    }
}
