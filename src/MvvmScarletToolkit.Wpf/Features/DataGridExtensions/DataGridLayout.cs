using System.ComponentModel;
using System.Windows.Controls;

namespace MvvmScarletToolkit
{
    /// <summary>
    /// The state of the columns of a <see cref="DataGrid"/>. Contains no WPF types, so that it can be serialized and stored by the application.
    /// </summary>
    public sealed class DataGridLayout
    {
        public IReadOnlyList<DataGridColumnLayout> Columns { get; init; } = [];
    }

    public sealed class DataGridColumnLayout
    {
        public required string Key { get; init; }

        public bool IsVisible { get; init; } = true;

        public int DisplayIndex { get; init; }

        public double Width { get; init; }

        public DataGridLengthUnitType WidthType { get; init; }

        public ListSortDirection? SortDirection { get; init; }

        /// <summary>
        /// the position of this column among the sorted columns
        /// </summary>
        public int? SortIndex { get; init; }

        /// <summary>
        /// The function of this column in a <see cref="DataGridSummaryBar"/>. Null leaves the column as it is, like in a layout stored before this existed.
        /// </summary>
        public DataGridSummaryFunction? SummaryFunction { get; init; }
    }
}
