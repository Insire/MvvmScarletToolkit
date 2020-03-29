using System;

namespace MvvmScarletToolkit
{
    internal static class Extensions
    {
        public static string GetGenericTypeName(this Type type)
        {
            var result = type.Name;

            while (type.GenericTypeArguments.Length > 0)
            {
                var typeName = type.GenericTypeArguments[0].Name;
                result = result.Replace("`1", $"<{typeName}>");
                type = type.GenericTypeArguments[0];
            }

            return result;
        }
    }
}
