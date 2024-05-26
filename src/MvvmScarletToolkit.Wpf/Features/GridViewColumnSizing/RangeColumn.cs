using System;
using System.Windows;
using System.Windows.Controls;

namespace MvvmScarletToolkit
{
    public sealed class RangeColumn : LayoutColumn
    {
        public static readonly DependencyProperty MinWidthProperty = DependencyProperty.RegisterAttached(
                "MinWidth",
                typeof(double),
                typeof(RangeColumn));

        public static readonly DependencyProperty MaxWidthProperty = DependencyProperty.RegisterAttached(
                "MaxWidth",
                typeof(double),
                typeof(RangeColumn));

        public static readonly DependencyProperty IsFillColumnProperty = DependencyProperty.RegisterAttached(
                "IsFillColumn",
                typeof(bool),
                typeof(RangeColumn));

        private RangeColumn()
        {
        }

        /// <summary>Helper for getting <see cref="MinWidthProperty"/> from <paramref name="obj"/>.</summary>
        /// <param name="obj"><see cref="DependencyObject"/> to read <see cref="MinWidthProperty"/> from.</param>
        /// <returns>MinWidth property value.</returns>
        [AttachedPropertyBrowsableForType(typeof(DependencyObject))]
        public static double GetMinWidth(DependencyObject obj)
        {
            return (double)obj.GetValue(MinWidthProperty);
        }

        /// <summary>Helper for setting <see cref="MinWidthProperty"/> on <paramref name="obj"/>.</summary>
        /// <param name="obj"><see cref="DependencyObject"/> to set <see cref="MinWidthProperty"/> on.</param>
        /// <param name="minWidth">MinWidth property value.</param>
        public static void SetMinWidth(DependencyObject obj, double minWidth)
        {
            obj.SetValue(MinWidthProperty, minWidth);
        }

        /// <summary>Helper for getting <see cref="MaxWidthProperty"/> from <paramref name="obj"/>.</summary>
        /// <param name="obj"><see cref="DependencyObject"/> to read <see cref="MaxWidthProperty"/> from.</param>
        /// <returns>MaxWidth property value.</returns>
        [AttachedPropertyBrowsableForType(typeof(DependencyObject))]
        public static double GetMaxWidth(DependencyObject obj)
        {
            return (double)obj.GetValue(MaxWidthProperty);
        }

        /// <summary>Helper for setting <see cref="MaxWidthProperty"/> on <paramref name="obj"/>.</summary>
        /// <param name="obj"><see cref="DependencyObject"/> to set <see cref="MaxWidthProperty"/> on.</param>
        /// <param name="maxWidth">MaxWidth property value.</param>
        public static void SetMaxWidth(DependencyObject obj, double maxWidth)
        {
            obj.SetValue(MaxWidthProperty, maxWidth);
        }

        /// <summary>Helper for getting <see cref="IsFillColumnProperty"/> from <paramref name="obj"/>.</summary>
        /// <param name="obj"><see cref="DependencyObject"/> to read <see cref="IsFillColumnProperty"/> from.</param>
        /// <returns>IsFillColumn property value.</returns>
        [AttachedPropertyBrowsableForType(typeof(DependencyObject))]
        public static bool GetIsFillColumn(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsFillColumnProperty);
        }

        /// <summary>Helper for setting <see cref="IsFillColumnProperty"/> on <paramref name="obj"/>.</summary>
        /// <param name="obj"><see cref="DependencyObject"/> to set <see cref="IsFillColumnProperty"/> on.</param>
        /// <param name="isFillColumn">IsFillColumn property value.</param>
        public static void SetIsFillColumn(DependencyObject obj, bool isFillColumn)
        {
            obj.SetValue(IsFillColumnProperty, isFillColumn);
        }

        public static bool IsRangeColumn(GridViewColumn column)
        {
            if (column is null)
            {
                return false;
            }
            return HasPropertyValue(column, MinWidthProperty) || HasPropertyValue(column, MaxWidthProperty) || HasPropertyValue(column, IsFillColumnProperty);
        }

        public static double? GetRangeMinWidth(GridViewColumn column)
        {
            return GetColumnWidth(column, MinWidthProperty);
        }

        public static double? GetRangeMaxWidth(GridViewColumn column)
        {
            return GetColumnWidth(column, MaxWidthProperty);
        }

        public static bool? GetRangeIsFillColumn(GridViewColumn column)
        {
            ArgumentNullException.ThrowIfNull(column, nameof(column));

            var value = column.ReadLocalValue(IsFillColumnProperty);
            if (value != null && value.GetType() == IsFillColumnProperty.PropertyType)
            {
                return (bool)value;
            }

            return null;
        }

        public static GridViewColumn ApplyWidth(GridViewColumn gridViewColumn, double minWidth, double width, double maxWidth)
        {
            return ApplyWidth(gridViewColumn, minWidth, width, maxWidth, false);
        }

        public static GridViewColumn ApplyWidth(GridViewColumn gridViewColumn, double minWidth, double width, double maxWidth, bool isFillColumn)
        {
            SetMinWidth(gridViewColumn, minWidth);
            gridViewColumn.SetCurrentValue(GridViewColumn.WidthProperty, width);
            SetMaxWidth(gridViewColumn, maxWidth);
            SetIsFillColumn(gridViewColumn, isFillColumn);

            return gridViewColumn;
        }
    }
}
