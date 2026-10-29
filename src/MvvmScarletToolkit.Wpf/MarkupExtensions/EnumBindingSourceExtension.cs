using System.ComponentModel;
using System.Reflection;
using System.Windows.Markup;

namespace MvvmScarletToolkit
{
    /// <summary>
    /// Create an itemsource from an enum type
    /// </summary>
    /// <example>
    /// <c><ItemsControl ItemsSource="{Binding Source={local:EnumBindingSource {x:Type local:MyEnumType}}}"/></c>
    /// </example>
    [MarkupExtensionReturnType(typeof(Array))]
    public sealed class EnumBindingSourceExtension : MarkupExtension
    {
        private Type? _enumType;

        [ConstructorArgument("enumType")]
        public Type? EnumType
        {
            get => _enumType;
            set
            {
                if (value != _enumType)
                {
                    if (value != null)
                    {
                        var enumType = Nullable.GetUnderlyingType(value) ?? value;

                        if (!enumType.IsEnum)
                        {
                            throw new ArgumentException("Type must be for an Enum.");
                        }
                    }

                    _enumType = value;
                }
            }
        }

        [ConstructorArgument("filterNonBrowseables")]
        public bool FilterNonBrowseables { get; set; }

        public EnumBindingSourceExtension()
        {
        }

        public EnumBindingSourceExtension(Type enumType)
            : this()
        {
            EnumType = enumType;
        }

        public EnumBindingSourceExtension(Type enumType, bool filterNonBrowseables)
            : this()
        {
            EnumType = enumType;
            FilterNonBrowseables = filterNonBrowseables;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (_enumType is null)
            {
                throw new InvalidOperationException("The EnumType must be specified.");
            }

            var actualEnumType = Nullable.GetUnderlyingType(_enumType) ?? _enumType;

            var values = FilterNonBrowseables
                ? GetBrowsableValues(actualEnumType)
                : Enum.GetValues(actualEnumType).Cast<object>().ToArray();

            // a nullable enum gets a leading null entry, so that the selection can be cleared
            var offset = actualEnumType == _enumType ? 0 : 1;
            var result = Array.CreateInstance(_enumType, values.Length + offset);
            for (var i = 0; i < values.Length; i++)
            {
                result.SetValue(values[i], i + offset);
            }

            return result;
        }

        //public static IEnumerable<TEnum> FilterEnumWithAttributeOf<TEnum, TAttribute>()
        //    where TEnum : struct
        //    where TAttribute : class
        //{
        //    foreach (var field in typeof(TEnum).GetFields(BindingFlags.GetField | BindingFlags.Public | BindingFlags.Static))
        //    {
        //        if (field.GetCustomAttributes(typeof(TAttribute), false).Length > 0)
        //        {
        //            yield return (TEnum)field.GetValue(null);
        //        }
        //    }
        //}

        //public static IEnumerable<TEnum> FilterEnumWithoutAttributeOf<TEnum, TAttribute>()
        //    where TEnum : struct
        //    where TAttribute : class
        //{
        //    foreach (var field in typeof(TEnum).GetFields(BindingFlags.GetField | BindingFlags.Public | BindingFlags.Static))
        //    {
        //        if (field.GetCustomAttributes(typeof(TAttribute), false).Length == 0)
        //        {
        //            yield return (TEnum)field.GetValue(null);
        //        }
        //    }
        //}

        /// <summary>
        /// values that are neither marked with [Browsable(false)] nor with [EditorBrowsable(EditorBrowsableState.Never)]
        /// </summary>
        private static object[] GetBrowsableValues(Type type)
        {
            return type.GetFields(BindingFlags.GetField | BindingFlags.Public | BindingFlags.Static)
                .Where(field => field.GetCustomAttribute<BrowsableAttribute>(false)?.Browsable != false
                    && field.GetCustomAttribute<EditorBrowsableAttribute>(false)?.State != EditorBrowsableState.Never)
                .Select(field => field.GetValue(null)!)
                .ToArray();
        }
    }
}
