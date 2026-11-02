using System.Globalization;

namespace MvvmScarletToolkit
{
    internal static class DataGridSummaryCalculator
    {
        private const DataGridSummaryFunction NumberFunctions = DataGridSummaryFunction.Sum | DataGridSummaryFunction.Average | OrderedFunctions;
        private const DataGridSummaryFunction OrderedFunctions = DataGridSummaryFunction.Minimum | DataGridSummaryFunction.Maximum | DataGridSummaryFunction.Count;

        /// <summary>
        /// What makes sense for values of <paramref name="type"/>: everything for numbers, the smallest and largest value for dates and times, and only counting rows for anything else
        /// </summary>
        /// <param name="type">null when the type of a column is not known, for example in an empty and untyped collection</param>
        public static DataGridSummaryFunction GetFunctions(Type? type)
        {
            type = type is null ? null : Nullable.GetUnderlyingType(type) ?? type;

            if (type is null || type.IsEnum)
            {
                return DataGridSummaryFunction.Count;
            }

            if (Type.GetTypeCode(type) is >= TypeCode.SByte and <= TypeCode.Decimal)
            {
                return NumberFunctions;
            }

            return type == typeof(DateTime) || type == typeof(DateTimeOffset) || type == typeof(DateOnly) || type == typeof(TimeOnly) || type == typeof(TimeSpan)
                ? OrderedFunctions
                : DataGridSummaryFunction.Count;
        }

        /// <summary>
        /// Null values are ignored, except by <see cref="DataGridSummaryFunction.Count"/>, which counts rows
        /// </summary>
        /// <returns>null when there is nothing to calculate</returns>
        public static object? Calculate(IReadOnlyList<object> items, string propertyPath, DataGridSummaryFunction function)
        {
            if (function == DataGridSummaryFunction.Count)
            {
                return items.Count;
            }

            var values = GetValues(items, propertyPath);

            return function switch
            {
                DataGridSummaryFunction.Sum => Sum(values, out _),
                DataGridSummaryFunction.Average => Average(values),
                DataGridSummaryFunction.Minimum => Select(values, -1),
                DataGridSummaryFunction.Maximum => Select(values, 1),
                _ => null,
            };
        }

        private static IEnumerable<object> GetValues(IReadOnlyList<object> items, string propertyPath)
        {
            foreach (var item in items)
            {
                var value = PropertyAccessor.Get(item.GetType(), propertyPath)?.GetValue(item);
                if (value is not null)
                {
                    yield return value;
                }
            }
        }

        private static decimal? Sum(IEnumerable<object> values, out int count)
        {
            var sum = 0m;
            count = 0;

            foreach (var value in values)
            {
                if (!IsNumber(value))
                {
                    continue;
                }

                sum += Convert.ToDecimal(value, CultureInfo.InvariantCulture);
                count++;
            }

            return count == 0 ? null : sum;
        }

        private static decimal? Average(IEnumerable<object> values)
        {
            var sum = Sum(values, out var count);

            return sum / count;
        }

        private static object? Select(IEnumerable<object> values, int direction)
        {
            object? result = null;

            foreach (var value in values)
            {
                if (value is IComparable comparable && (result is null || Math.Sign(comparable.CompareTo(result)) == direction))
                {
                    result = value;
                }
            }

            return result;
        }

        private static bool IsNumber(object value)
        {
            return value is sbyte or byte or short or ushort or int or uint or long or ulong or float or double or decimal;
        }
    }
}
