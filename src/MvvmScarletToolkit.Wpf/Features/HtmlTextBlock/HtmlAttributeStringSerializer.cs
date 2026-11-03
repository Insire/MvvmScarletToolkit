namespace MvvmScarletToolkit.Wpf
{
    /// <summary>
    /// Reads html attributes like <c>name='value'</c>, <c>name="value"</c>, <c>name=value</c> and <c>name</c>.
    /// Names are returned in lower case, quotes are removed from values and attributes without a value get <c>TRUE</c>.
    /// </summary>
    internal sealed class HtmlAttributeStringSerializer : IPropertySerializer
    {
        private const string ValuelessAttribute = "TRUE";

        public IEnumerable<Tuple<string, string>> StringToProperty(string propertyString)
        {
            var text = propertyString ?? string.Empty;
            var index = 0;

            while (true)
            {
                SkipWhiteSpace(text, ref index);
                if (index >= text.Length)
                {
                    yield break;
                }

                var nameStart = index;
                while (index < text.Length && !char.IsWhiteSpace(text[index]) && text[index] != '=')
                {
                    index++;
                }

                var name = text[nameStart..index];
                if (name.Length == 0)
                {
                    // a stray '=' without a name, skip it so parsing always moves forward
                    index++;
                    continue;
                }

                var afterName = index;
                SkipWhiteSpace(text, ref index);

                if (index >= text.Length || text[index] != '=')
                {
                    index = afterName;
                    yield return new Tuple<string, string>(name.ToLowerInvariant(), ValuelessAttribute);
                    continue;
                }

                index++; // '='
                SkipWhiteSpace(text, ref index);

                yield return new Tuple<string, string>(name.ToLowerInvariant(), ReadValue(text, ref index));
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

        private static string ReadValue(string text, ref int index)
        {
            if (index >= text.Length)
            {
                return string.Empty;
            }

            var quote = text[index];
            if (quote is '\'' or '"')
            {
                var start = index + 1;
                var end = text.IndexOf(quote, start);
                if (end == -1)
                {
                    // unterminated, take the rest
                    index = text.Length;
                    return text[start..];
                }

                index = end + 1;
                return text[start..end];
            }

            var valueStart = index;
            while (index < text.Length && !char.IsWhiteSpace(text[index]))
            {
                index++;
            }

            return text[valueStart..index];
        }

        private static void SkipWhiteSpace(string text, ref int index)
        {
            while (index < text.Length && char.IsWhiteSpace(text[index]))
            {
                index++;
            }
        }
    }
}
