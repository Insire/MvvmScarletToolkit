using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows.Markup;

namespace MvvmScarletToolkit
{
    // Usage: ItemsSource="{Binding Source={local:EnumBindingSource {x:Type local:EnumType}}}"
    [MarkupExtensionReturnType(typeof(Array))]
    public sealed class EnumBindingSourceExtension : MarkupExtension
    {
        private Type _enumType;

        [ConstructorArgument("enumType")]
        public Type EnumType
        {
            get { return _enumType; }
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

            if (FilterNonBrowseables)
            {
                var filteredValues = FilterEnumWithoutAttributeOf<BrowsableAttribute>(actualEnumType)
                    .Concat(FilterEnumWithoutAttributeOf<EditorBrowsableAttribute>(actualEnumType))
                    .Distinct()
                    .ToArray();

                var tempArray = Array.CreateInstance(actualEnumType, filteredValues.Length + 1);
                filteredValues.CopyTo(tempArray, 1);
                return tempArray;
            }
            else
            {
                var enumValues = Enum.GetValues(actualEnumType);
                if (actualEnumType == _enumType)
                {
                    return enumValues;
                }

                var tempArray = Array.CreateInstance(actualEnumType, enumValues.Length + 1);
                enumValues.CopyTo(tempArray, 1);
                return tempArray;
            }
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

        private static IEnumerable<object> FilterEnumWithoutAttributeOf<TAttribute>(Type type)
            where TAttribute : class
        {
            foreach (var field in type.GetFields(BindingFlags.GetField | BindingFlags.Public | BindingFlags.Static))
            {
                if (field.GetCustomAttributes(typeof(TAttribute), false).Length == 0)
                {
                    yield return field.GetValue(null);
                }
            }
        }
    }
}
