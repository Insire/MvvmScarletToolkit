using System.Windows.Controls;
using System.Windows.Data;

namespace MvvmScarletToolkit
{
    internal static class DataGridColumnExtensions
    {
        /// <summary>
        /// the path of the property a column displays, or null when it has none, like an unbound template column
        /// </summary>
        public static string? GetPropertyPath(this DataGridColumn column)
        {
            var path = (column.ClipboardContentBinding as Binding)?.Path?.Path;
            if (!string.IsNullOrEmpty(path))
            {
                return path;
            }

            return string.IsNullOrEmpty(column.SortMemberPath) ? null : column.SortMemberPath;
        }

        /// <summary>
        /// a name for a column, that doesn't depend on its localized header if possible
        /// </summary>
        public static string GetKey(this DataGridColumn column, int index)
        {
            return column.GetPropertyPath() ?? column.Header?.ToString() ?? $"Column{index}";
        }

        public static string GetCaption(this DataGridColumn column, int index)
        {
            return column.Header?.ToString() ?? column.GetKey(index);
        }
    }
}
