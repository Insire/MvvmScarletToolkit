using System.Data;
using System.Reflection;

namespace MvvmScarletToolkit
{
    /// <summary>
    /// Mappers for <see cref="IDataGridDataSource.ToEnumerable{TResult}"/>
    /// </summary>
    public static class DataRowMapper
    {
        /// <summary>
        /// Creates a mapper that matches columns to settable properties of <typeparamref name="T"/> by name, using reflection.
        /// Columns and properties without a match are skipped.
        /// </summary>
        /// <remarks>Throws an <see cref="InvalidOperationException"/> naming the column, when a value doesn't fit its property.</remarks>
        public static Func<DataRow, T> ByPropertyName<T>()
            where T : new()
        {
            var properties = typeof(T)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => p.SetMethod?.IsPublic == true && p.GetIndexParameters().Length == 0)
                .ToArray();

            return row =>
            {
                var result = new T();

                foreach (var property in properties)
                {
                    var index = row.Table.Columns.IndexOf(property.Name);
                    if (index >= 0)
                    {
                        property.SetValue(result, Convert(row[index], property, row.Table.Columns[index]));
                    }
                }

                return result;
            };
        }

        private static object? Convert(object value, PropertyInfo property, DataColumn column)
        {
            var type = property.PropertyType;

            if (value is DBNull)
            {
                if (!type.IsValueType || Nullable.GetUnderlyingType(type) is not null)
                {
                    return null;
                }
            }
            else if ((Nullable.GetUnderlyingType(type) ?? type).IsInstanceOfType(value))
            {
                return value;
            }

            throw new InvalidOperationException($"The value of column {column.ColumnName} can't be assigned to {property.DeclaringType?.Name}.{property.Name} of type {type.Name}.");
        }
    }
}
