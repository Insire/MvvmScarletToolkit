using System.Windows;
using System.Windows.Controls;

namespace MvvmScarletToolkit
{
    public sealed class FixedColumn : LayoutColumn
    {
        public static readonly DependencyProperty WidthProperty = DependencyProperty.RegisterAttached(
                "Width",
                typeof(double),
                typeof(FixedColumn));

        private FixedColumn()
        {
        }

        /// <summary>Helper for getting <see cref="WidthProperty"/> from <paramref name="obj"/>.</summary>
        /// <param name="obj"><see cref="DependencyObject"/> to read <see cref="WidthProperty"/> from.</param>
        /// <returns>Width property value.</returns>
        [AttachedPropertyBrowsableForType(typeof(DependencyObject))]
        public static double GetWidth(DependencyObject obj)
        {
            return (double)obj.GetValue(WidthProperty);
        }

        /// <summary>Helper for setting <see cref="WidthProperty"/> on <paramref name="obj"/>.</summary>
        /// <param name="obj"><see cref="DependencyObject"/> to set <see cref="WidthProperty"/> on.</param>
        /// <param name="width">Width property value.</param>
        public static void SetWidth(DependencyObject obj, double width)
        {
            obj.SetValue(WidthProperty, width);
        }

        public static bool IsFixedColumn(GridViewColumn column)
        {
            if (column is null)
            {
                return false;
            }

            return HasPropertyValue(column, WidthProperty);
        }

        public static double? GetFixedWidth(GridViewColumn column)
        {
            return GetColumnWidth(column, WidthProperty);
        }

        public static GridViewColumn ApplyWidth(GridViewColumn gridViewColumn, double width)
        {
            SetWidth(gridViewColumn, width);
            return gridViewColumn;
        }
    }
}
