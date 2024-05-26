using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace MvvmScarletToolkit
{
    public abstract class MultiConverterMarkupExtension<T> : MarkupExtension, IMultiValueConverter
        where T : class, IMultiValueConverter, new()
    {
        private static T? _converter;

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return _converter ??= new T();
        }

        public abstract object Convert(object[] values, Type targetType, object parameter, CultureInfo culture);

        public virtual object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
        {
            // According to https://msdn.microsoft.com/en-us/library/system.windows.data.ivalueconverter.convertback(v=vs.110).aspx#Anchor_1
            // (kudos Scott Chamberlain), if you do not support a conversion
            // back you should return a Binding.DoNothing or a DependencyProperty.UnSetProperty
            return new[] { Binding.DoNothing };
        }
    }
}
