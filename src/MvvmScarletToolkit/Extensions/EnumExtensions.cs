using System.Diagnostics.CodeAnalysis;

namespace MvvmScarletToolkit
{
    public static class EnumExtensions
    {
        /// <summary>
        /// Returns the first attribute of type <typeparamref name="T"/> declared on the enum member of <paramref name="value"/>.
        /// </summary>
        /// <remarks>
        /// Not trim safe, because the enum type is only known at runtime. Prefer <see cref="GetAttributeOfType{TEnum, TAttribute}(TEnum)"/>.
        /// </remarks>
        [RequiresUnreferencedCode("The enum type is only known at runtime, so its fields might be trimmed. Use GetAttributeOfType<TEnum, TAttribute> instead.")]
        public static T? GetAttributeOfType<T>(this Enum value)
            where T : Attribute
        {
            return GetAttribute<T>(value.GetType().GetField(value.ToString()));
        }

        /// <summary>
        /// Returns the first attribute of type <typeparamref name="TAttribute"/> declared on the enum member of <paramref name="value"/>.
        /// </summary>
        public static TAttribute? GetAttributeOfType<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] TEnum, TAttribute>(this TEnum value)
            where TEnum : Enum
            where TAttribute : Attribute
        {
            return GetAttribute<TAttribute>(typeof(TEnum).GetField(value.ToString()));
        }

        private static T? GetAttribute<T>(System.Reflection.FieldInfo? field)
            where T : Attribute
        {
            if (field is null)
            {
                return default;
            }

            var attributes = field.GetCustomAttributes(typeof(T), false);

            return (attributes.Length > 0) ? (T)attributes[0] : default;
        }
    }
}
