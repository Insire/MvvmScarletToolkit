using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Markup;

namespace MvvmScarletToolkit
{
    /// <summary>
    /// turns an Enumerable of strings into a single string
    /// </summary>
    /// <remarks>
    /// <c>xmlns:mvvm="http://SoftThorn.MvvmScarletToolkit.com/winfx/xaml/shared"</c>
    /// </remarks>
    [ValueConversion(typeof(string[]), typeof(string))]
    public sealed class Flatten : ConverterMarkupExtension<Flatten>
    {
        [ConstructorArgument("separator")]
        public string Separator { get; set; } = ", ";

        public override object Convert(object? value, Type? targetType, object? parameter, CultureInfo? culture)
        {
            if (value is IEnumerable<string> strings)
            {
                return string.Join(Separator, strings.Where(s => !string.IsNullOrWhiteSpace(s)));
            }

            return Binding.DoNothing;
        }
    }
}
