using System;

namespace MvvmScarletToolkit
{
    public static class EnumExtensions
    {
        public static T? GetAttributeOfType<T>(this Enum value)
            where T : Attribute
        {
            var type = value.GetType();
            var info = type.GetMember(value.ToString());

            if (info.Length > 0)
            {
                var attributes = info[0].GetCustomAttributes(typeof(T), false);

                return (attributes.Length > 0) ? (T)attributes[0] : default;
            }
            return default;
        }
    }
}
