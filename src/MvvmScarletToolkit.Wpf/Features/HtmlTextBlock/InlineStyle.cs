using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace MvvmScarletToolkit.Wpf
{
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

        public bool TryGetLength(string property, out double length)
        {
            length = double.NaN;

            return _declarations.TryGetValue(property, out var value)
                && TryParseLength(value, out length);
        }

        public bool TryGetThickness(string property, out Thickness thickness)
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

            thickness = new Thickness(left, top, right, bottom);
            return true;
        }

        public bool TryGetBaselineAlignment(string property, out BaselineAlignment alignment)
        {
            alignment = BaselineAlignment.Baseline;

            if (!_declarations.TryGetValue(property, out var value))
            {
                return false;
            }

            switch (value.ToLowerInvariant())
            {
                case "baseline":
                    alignment = BaselineAlignment.Baseline;
                    return true;

                case "top":
                    alignment = BaselineAlignment.Top;
                    return true;

                case "middle":
                    alignment = BaselineAlignment.Center;
                    return true;

                case "bottom":
                    alignment = BaselineAlignment.Bottom;
                    return true;

                case "text-top":
                    alignment = BaselineAlignment.TextTop;
                    return true;

                case "text-bottom":
                    alignment = BaselineAlignment.TextBottom;
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

        public bool TryGetColor(string property, out Color color)
        {
            color = default;

            return _declarations.TryGetValue(property, out var value)
                && TryParseColor(value, out color);
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
        /// Reads named colors and #rgb, #rrggbb and #aarrggbb.
        /// </summary>
        public static bool TryParseColor(string? value, out Color color)
        {
            color = default;

            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            try
            {
                if (ColorConverter.ConvertFromString(value.Trim()) is Color parsed)
                {
                    color = parsed;
                    return true;
                }
            }
            catch (FormatException)
            {
                // invalid colors are ignored
            }
            catch (NotSupportedException)
            {
                // invalid colors are ignored
            }

            return false;
        }
    }
}
