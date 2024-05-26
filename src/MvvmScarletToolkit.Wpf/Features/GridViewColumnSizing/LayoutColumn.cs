using System;
using System.Windows;
using System.Windows.Controls;

namespace MvvmScarletToolkit
{
    public abstract class LayoutColumn
    {
        protected static bool HasPropertyValue(GridViewColumn column, DependencyProperty dp)
        {
            ArgumentNullException.ThrowIfNull(column, nameof(column));

            var value = column.ReadLocalValue(dp);
            return value != null && value.GetType() == dp.PropertyType;
        }

        protected static double? GetColumnWidth(GridViewColumn column, DependencyProperty dp)
        {
            ArgumentNullException.ThrowIfNull(column, nameof(column));

            var value = column.ReadLocalValue(dp);
            if (value != null && value.GetType() == dp.PropertyType)
            {
                return (double)value;
            }

            return null;
        }
    }
}
