using System.Collections.Generic;

namespace MvvmScarletToolkit.Wpf
{
    internal interface IParamParser
    {
        /// <summary>
        /// Uses to serialize the properties to string.
        /// </summary>
        IPropertySerializer Serializer { get; }

        /// <summary>
        /// Convert a dictionary contining key value pairs to ParamString.
        /// </summary>
        /// <param name="param">An array of key and value pairs</param>
        /// <returns>Param String containing both key and value, e.g. key:"value"</returns>
        string DictionaryToString(Dictionary<string, string> dictionary);

        /// <summary>
        /// Take a Paramstring and return a dictionary containing
        /// the paramstring's key and value.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="paramDic"></param>
        Dictionary<string, string> StringToDictionary(string paramString);
    }
}
