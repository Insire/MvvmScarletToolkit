using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace MvvmScarletToolkit
{
    /// <summary>
    /// compiled read access to a property path like "Address.City". Indexers and attached properties are not supported.
    /// </summary>
    internal sealed class PropertyAccessor
    {
        private static readonly ConcurrentDictionary<(Type Type, string Path), PropertyAccessor?> _cache = new();

        private readonly Func<object, object?>[] _segments;

        public Type PropertyType { get; }

        private PropertyAccessor(Func<object, object?>[] segments, Type propertyType)
        {
            _segments = segments;
            PropertyType = propertyType;
        }

        public static PropertyAccessor? Get(Type type, string path)
        {
            return _cache.GetOrAdd((type, path), static key => Create(key.Type, key.Path));
        }

        public object? GetValue(object item)
        {
            object? current = item;
            foreach (var segment in _segments)
            {
                if (current is null)
                {
                    return null;
                }

                current = segment(current);
            }

            return current;
        }

        private static PropertyAccessor? Create(Type type, string path)
        {
            var names = path.Split('.');
            var segments = new Func<object, object?>[names.Length];
            var current = type;

            for (var i = 0; i < names.Length; i++)
            {
                var property = current.GetProperty(names[i], BindingFlags.Instance | BindingFlags.Public);
                if (property?.GetMethod is null || property.GetIndexParameters().Length > 0)
                {
                    return null;
                }

                segments[i] = Compile(current, property);
                current = property.PropertyType;
            }

            return new PropertyAccessor(segments, current);
        }

        private static Func<object, object?> Compile(Type type, PropertyInfo property)
        {
            var parameter = Expression.Parameter(typeof(object));
            var body = Expression.Convert(Expression.Property(Expression.Convert(parameter, type), property), typeof(object));

            return Expression.Lambda<Func<object, object?>>(body, parameter).Compile();
        }
    }
}
