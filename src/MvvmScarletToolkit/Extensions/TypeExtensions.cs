namespace MvvmScarletToolkit
{
    public static class TypeExtensions
    {
        public static string GetGenericTypeName(this Type type)
        {
            // a type nested in a generic type has the arguments of its parent, but no arity in its name
            var arity = type.Name.IndexOf('`');
            if (arity < 0)
            {
                return type.Name;
            }

            return $"{type.Name.Substring(0, arity)}<{string.Join(", ", type.GenericTypeArguments.Select(GetGenericTypeName))}>";
        }
    }
}
