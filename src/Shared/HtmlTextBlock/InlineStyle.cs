using System.Globalization;

namespace MvvmScarletToolkit.Html
{
    /// <summary>
    /// The css <c>vertical-align</c> values an <c>img</c> supports.
    /// </summary>
    internal enum VerticalAlign
    {
        Baseline,
        Top,
        Middle,
        Bottom,
        TextTop,
        TextBottom,
    }

    /// <summary>
    /// A css margin, without depending on the thickness type of a UI framework.
    /// </summary>
    internal readonly record struct CssThickness(double Left, double Top, double Right, double Bottom);

    /// <summary>
    /// Parses a color for one UI framework, e.g. named colors and #rgb, #rrggbb and #aarrggbb.
    /// </summary>
    internal delegate bool ColorParser<TColor>(string value, out TColor color);

    /// <summary>
    /// Reads the declarations of an html <c>style</c> attribute, like <c>width: 16px; margin: 0 4px</c>.
    /// Unknown properties and invalid values are ignored, numbers are read independent of the current culture.
    /// </summary>
    internal sealed class InlineStyle
    {
        private readonly Dictionary<string, string> _declarations;

        private InlineStyle(Dictionary<string, string> declarations)
        {
            _declarations = declarations;
        }

        public static InlineStyle Parse(string? style)
        {
            var declarations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (!string.IsNullOrWhiteSpace(style))
            {
                foreach (var declaration in style.Split(';'))
                {
                    var separator = declaration.IndexOf(':');
                    if (separator <= 0)
                    {
                        continue;
                    }

                    var name = declaration[..separator].Trim();
                    var value = declaration[(separator + 1)..].Trim();
                    if (name.Length > 0 && value.Length > 0)
                    {
                        // like in css, a later declaration wins
                        declarations[name] = value;
                    }
                }
            }

            return new InlineStyle(declarations);
        }

        /// <summary>
        /// Returns the raw value of a declaration, e.g. a color for the UI framework to parse.
        /// </summary>
        public string? Get(string property)
        {
            return _declarations.TryGetValue(property, out var value) ? value : null;
        }

        public bool TryGetLength(string property, out double length)
        {
            length = double.NaN;

            return _declarations.TryGetValue(property, out var value)
                && TryParseLength(value, out length);
        }

        public bool TryGetThickness(string property, out CssThickness thickness)
        {
            thickness = default;

            if (!_declarations.TryGetValue(property, out var value))
            {
                return false;
            }

            var parts = value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length is 0 or > 4)
            {
                return false;
            }

            var lengths = new double[parts.Length];
            for (var i = 0; i < parts.Length; i++)
            {
                if (!TryParseLength(parts[i], out lengths[i]))
                {
                    return false;
                }
            }

            // css order is top, right, bottom, left. Missing values repeat their opposite side
            var top = lengths[0];
            var right = lengths.Length > 1 ? lengths[1] : top;
            var bottom = lengths.Length > 2 ? lengths[2] : top;
            var left = lengths.Length > 3 ? lengths[3] : right;

            thickness = new CssThickness(left, top, right, bottom);
            return true;
        }

        public bool TryGetVerticalAlign(string property, out VerticalAlign alignment)
        {
            alignment = VerticalAlign.Baseline;

            if (!_declarations.TryGetValue(property, out var value))
            {
                return false;
            }

            switch (value.ToLowerInvariant())
            {
                case "baseline":
                    alignment = VerticalAlign.Baseline;
                    return true;

                case "top":
                    alignment = VerticalAlign.Top;
                    return true;

                case "middle":
                    alignment = VerticalAlign.Middle;
                    return true;

                case "bottom":
                    alignment = VerticalAlign.Bottom;
                    return true;

                case "text-top":
                    alignment = VerticalAlign.TextTop;
                    return true;

                case "text-bottom":
                    alignment = VerticalAlign.TextBottom;
                    return true;

                default:
                    return false;
            }
        }

        public bool TryGetOpacity(string property, out double opacity)
        {
            opacity = 1;

            if (!_declarations.TryGetValue(property, out var value)
                || !double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
                || double.IsNaN(parsed))
            {
                return false;
            }

            opacity = Math.Clamp(parsed, 0, 1);
            return true;
        }

        /// <summary>
        /// Reads a length as a plain number or in px. Other units and negative values are rejected.
        /// </summary>
        public static bool TryParseLength(string? value, out double length)
        {
            length = double.NaN;

            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            var text = value.Trim();
            if (text.EndsWith("px", StringComparison.OrdinalIgnoreCase))
            {
                text = text[..^2].TrimEnd();
            }

            if (!double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
                || double.IsNaN(parsed)
                || double.IsInfinity(parsed)
                || parsed < 0)
            {
                return false;
            }

            length = parsed;
            return true;
        }

        /// <summary>
        /// Reads a length from the style, or else from the attribute of the same name. Like in browsers, the style wins.
        /// </summary>
        public static double GetLength(HtmlTag tag, InlineStyle style, string property)
        {
            if (style.TryGetLength(property, out var length))
            {
                return length;
            }

            return TryParseLength(tag.Get(property), out length)
                ? length
                : double.NaN;
        }

        /// <summary>
        /// The size to decode an image at, or <see langword="null"/> when <paramref name="length"/> is unknown or out of range.
        /// </summary>
        public static uint? ToDecodeSize(double length)
        {
            if (double.IsNaN(length) || length < 1 || length > uint.MaxValue)
            {
                return null;
            }

            return (uint)Math.Ceiling(length);
        }
    }
}
