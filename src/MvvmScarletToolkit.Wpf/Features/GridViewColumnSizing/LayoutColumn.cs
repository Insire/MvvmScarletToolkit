using System;
using System.Windows;
using System.Windows.Controls;

namespace MvvmScarletToolkit
{
    public abstract class LayoutColumn
    {
        protected static bool HasPropertyValue(GridViewColumn column, DependencyProperty dp)
        {
            if (column is null)
            {
                throw new ArgumentNullException(nameof(column));
            }

            var value = column.ReadLocalValue(dp);
            if (value != null && value.GetType() == dp.PropertyType)
            {
                return true;
            }

            return false;
        }

        protected static double? GetColumnWidth(GridViewColumn column, DependencyProperty dp)
        {
            if (column is null)
            {
                throw new ArgumentNullException(nameof(column));
            }

            var value = column.ReadLocalValue(dp);
            if (value != null && value.GetType() == dp.PropertyType)
            {
                return (double)value;
            }

            return null;
        }
    }
}
