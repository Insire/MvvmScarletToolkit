using System;
using System.Collections.Generic;
using System.Linq;

namespace MvvmScarletToolkit.Wpf
{
    internal sealed class ParamParser : IParamParser
    {
        public IPropertySerializer Serializer { get; }

        public ParamParser(IPropertySerializer serializer)
        {
            Serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        }

        public string DictionaryToString(Dictionary<string, string> dictionary)
        {
            return Serializer.PropertyToString(dictionary.Keys.Select(p => new Tuple<string, string>(p, dictionary[p])));
        }

        public Dictionary<string, string> StringToDictionary(string @string)
        {
            var result = new Dictionary<string, string>();

            foreach (var tup in Serializer.StringToProperty(@string))
            {
                result[tup.Item1] = tup.Item2;
            }

            return result;
        }
    }
}
