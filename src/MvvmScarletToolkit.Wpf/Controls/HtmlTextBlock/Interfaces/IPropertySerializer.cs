using System;
using System.Collections.Generic;

namespace MvvmScarletToolkit.Wpf
{
    internal interface IPropertySerializer
    {
        /// <summary>
        /// Convert a dictionary contining key value pairs to property string.
        /// </summary>
        /// <param name="properties">An array of key and value pairs</param>
        /// <returns>Param String containing both key and value, e.g. key:"value"</returns>
        string PropertyToString(IEnumerable<Tuple<string, string>> properties);

        /// <summary>
        /// Take a property string and return a list of tuple containing
        /// the its key and value.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="paramDic"></param>
        IEnumerable<Tuple<string, string>> StringToProperty(string propertyString);
    }
}
