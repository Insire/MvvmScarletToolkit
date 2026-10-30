using System;
using System.Collections.Generic;

namespace MvvmScarletToolkit
{
    /// <summary>
    /// The rows of a data grid, and a description of its visible columns
    /// </summary>
    public sealed class DataGridItems<TItem>
    {
        public IReadOnlyList<DataGridColumnInfo> Columns { get; }

        public IReadOnlyList<TItem> Items { get; }

        public DataGridItems(IReadOnlyList<DataGridColumnInfo> columns, IReadOnlyList<TItem> items)
        {
            Columns = columns ?? throw new ArgumentNullException(nameof(columns));
            Items = items ?? throw new ArgumentNullException(nameof(items));
        }
    }

    public sealed class DataGridColumnInfo
    {
        /// <summary>
        /// Unique within a grid. This is the binding path if there is one, so it doesn't depend on a localized header.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The text of the column header
        /// </summary>
        public string Caption { get; }

        public DataGridColumnInfo(string name, string caption)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Caption = caption ?? throw new ArgumentNullException(nameof(caption));
        }
    }
}
