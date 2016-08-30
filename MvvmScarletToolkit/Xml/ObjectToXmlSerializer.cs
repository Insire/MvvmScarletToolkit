using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml.Serialization;

namespace MvvmScarletToolkit
{
    public static class ObjectSerializer
    {
        public static void SaveCollection<T>(Collection<T> Items, FileInfo info)
        {
            if (Items.Count > 0)
            {
                var s = new XmlSerializer(typeof(Collection<T>));
                using (var streamWriter = new StreamWriter(info.FullName))
                    s.Serialize(streamWriter, Items);
            }
        }

        public static void Save<T>(T item, FileInfo info)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item), "can't serialze null");

            var serializer = new XmlSerializer(typeof(T));

            using (var writer = new StreamWriter(info.FullName))
                serializer.Serialize(writer, item);
        }

        public static Collection<T> LoadCollection<T>(FileInfo info)
        {
            var result = default(Collection<T>);

            if (info.Exists)
            {
                var serializer = new XmlSerializer(typeof(Collection<T>));
                using (var streamReader = new StreamReader(info.FullName))
                {
                    var item = serializer.Deserialize(streamReader) as Collection<T>;
                    if (item != null)
                        return result = item;
                }
            }
            return result;
        }

        public static T Load<T>(FileInfo info) where T : class, new()
        {
            var result = default(T);

            if (info.Exists)
            {
                var serializer = new XmlSerializer(typeof(T));
                using (var streamReader = new StreamReader(info.FullName))
                {
                    var item = serializer.Deserialize(streamReader) as T;

                    if (item != null)
                        return result = item;
                }
            }
            return result;
        }

        public static string Save<T>(T item)
        {
            var serializer = new XmlSerializer(typeof(T));

            using (var textWriter = new StringWriter())
            {
                serializer.Serialize(textWriter, item);
                return textWriter.ToString();
            }
        }

        public static T Load<T>(string info) where T : class, new()
        {
            var result = default(T);

            var serializer = new XmlSerializer(typeof(T));
            using (var stringReader = new StringReader(info))
            {
                var item = serializer.Deserialize(stringReader) as T;

                if (item != null)
                    return result = item;
            }

            return result;
        }
    }
}