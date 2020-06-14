using System;
using System.Collections.Generic;

namespace MvvmScarletToolkit.Wpf
{
    internal sealed class HtmlAttributeStringSerializer : IPropertySerializer
    {
        private const char Quote = '\'';

        public IEnumerable<Tuple<string, string>> StringToProperty(string propertyString)
        {
            var working = propertyString;
            var name = string.Empty;
            var value = string.Empty;

            while (working != string.Empty)
            {
                LocateNextVariable(ref working, ref name, ref value);
                yield return new Tuple<string, string>(name, value);
            }
        }

        public string PropertyToString(IEnumerable<Tuple<string, string>> properties)
        {
            var retVal = string.Empty;

            foreach (var prop in properties)
            {
                retVal += string.Format(" {0}=\"{1}\"", prop.Item1, prop.Item2);
            }

            return retVal;
        }

        private static void LocateNextVariable(ref string working, ref string name, ref string value)
        {
            working = working.Trim();

            var pos1 = working.IndexOf('=');
            if (pos1 != -1)
            {
                name = working.Substring(0, pos1);
                var quoteIndex = working.IndexOf(Quote);
                var spaceIndex = working.IndexOf(' ');
                var equalsIndex = working.IndexOf('=');

                if (spaceIndex == -1)
                {
                    spaceIndex = equalsIndex + 1;
                }

                if ((quoteIndex == -1) || (quoteIndex > spaceIndex))
                {
                    value = working.Substring(equalsIndex + 1, working.Length - equalsIndex - 1);
                    spaceIndex = working.IndexOf(' ');

                    if (spaceIndex == -1)
                    {
                        working = string.Empty;
                    }
                    else
                    {
                        working = working.Substring(spaceIndex + 1, working.Length - spaceIndex - 1);
                    }
                }
                else
                {
                    working = working.Substring(quoteIndex + 1, working.Length - quoteIndex - 1);
                    quoteIndex = working.IndexOf(Quote);

                    if (quoteIndex != -1)
                    {
                        value = working.Substring(0, quoteIndex);
                        working = working.Substring(quoteIndex + 1, working.Length - quoteIndex - 1);
                    }
                }
            }
            else
            {
                name = working;
                value = "TRUE";
                working = string.Empty;
            }
        }
    }
}
