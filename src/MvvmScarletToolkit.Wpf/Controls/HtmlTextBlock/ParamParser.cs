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

        public string DictionaryToString(Dictionary<string, string> paramDic)
        {
            return Serializer.PropertyToString(paramDic.Keys.Select(p => new Tuple<string, string>(p, paramDic[p])));
        }

        public Dictionary<string, string> StringToDictionary(string paramString)
        {
            var retDic = new Dictionary<string, string>();

            foreach (var tup in Serializer.StringToProperty(paramString))
            {
                if (!retDic.ContainsKey(tup.Item1))
                {
                    retDic.Add(tup.Item1, tup.Item2);
                }
                else
                {
                    retDic[tup.Item1] = tup.Item2;
                }
            }

            return retDic;
        }
    }
}
