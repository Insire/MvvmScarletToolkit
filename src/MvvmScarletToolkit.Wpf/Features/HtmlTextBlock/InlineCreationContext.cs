using System.Windows.Media;

namespace MvvmScarletToolkit.Wpf
{
    internal sealed class InlineCreationContext
    {
        private readonly List<HtmlTag> _activeStyle = new List<HtmlTag>();

        public bool Bold { get; private set; }
        public bool Italic { get; private set; }
        public bool Underline { get; private set; }
        public bool SubScript { get; private set; }
        public bool SuperScript { get; private set; }
        public string? HyperLink { get; private set; }
        public Color? Foreground { get; private set; }

        /// <summary>1 to 6 inside h1 to h6, otherwise 0. The innermost heading wins.</summary>
        public int HeadingLevel { get; private set; }

        /// <summary>Inside code or pre.</summary>
        public bool Monospace { get; private set; }

        private readonly List<ListState> _lists = new List<ListState>();

        public InlineCreationContext()
        {
            Foreground = null;
            HyperLink = "";
        }

        public void UpdateStyle(HtmlTag tag)
        {
            if (!tag.IsEndTag)
            {
                _activeStyle.Add(tag);
            }
            else
            {
                for (var i = _activeStyle.Count - 1; i >= 0; i--)
                {
                    if (_activeStyle[i].EndTagName == tag.Name)
                    {
                        _activeStyle.RemoveAt(i);
                        break;
                    }
                }
            }

            ResetSettings();
            UpdateSettings();
        }

        private void ResetSettings()
        {
            Bold = false;
            Italic = false;
            Underline = false;
            SubScript = false;
            SuperScript = false;
            Foreground = null;
            HyperLink = "";
            HeadingLevel = 0;
            Monospace = false;
        }

        /// <summary>
        /// Active tags are applied from the outside in, so the innermost valid color wins.
        /// </summary>
        private void ApplyColor(bool isValid, Color color)
        {
            if (isValid)
            {
                Foreground = color;
            }
        }

        private void UpdateSettings()
        {
            for (var i = 0; i < _activeStyle.Count; i++)
            {
                var tag = _activeStyle[i];
                switch (tag.Name)
                {
                    case "b":
                        Bold = true;
                        break;

                    case "i":
                        Italic = true;
                        break;

                    case "u":
                        Underline = true;
                        break;

                    case "sub":
                        SubScript = true;
                        break;

                    case "sup":
                        SuperScript = true;
                        break;

                    case "a":
                        if (tag.Contains("href"))
                        {
                            HyperLink = tag["href"];
                        }

                        break;

                    case "font":
                    case "color":
                        ApplyColor(InlineStyle.TryParseColor(tag.Get("color"), out var tagColor), tagColor);
                        break;

                    case "span":
                        ApplyColor(InlineStyle.Parse(tag.Get("style")).TryGetColor("color", out var spanColor), spanColor);
                        break;

                    case "h1":
                    case "h2":
                    case "h3":
                    case "h4":
                    case "h5":
                    case "h6":
                        Bold = true;
                        HeadingLevel = tag.Name[1] - '0';
                        break;

                    case "code":
                    case "pre":
                        Monospace = true;
                        break;
                }
            }
        }

        /// <summary>
        /// Tracks ul, ol and li tags.
        /// </summary>
        /// <returns>The bullet or number to show for an opening li tag, otherwise <see langword="null"/>.</returns>
        public string? UpdateList(HtmlTag tag)
        {
            switch (tag.Name)
            {
                case "ul":
                case "ol":
                    _lists.Add(new ListState(tag.Name == "ol"));
                    return null;

                case "/ul":
                case "/ol":
                    if (_lists.Count > 0)
                    {
                        _lists.RemoveAt(_lists.Count - 1);
                    }

                    return null;

                case "li" when !tag.IsEndTag:
                    // an li outside of a list is shown like one in an ul
                    var list = _lists.Count > 0 ? _lists[^1] : new ListState(false);
                    list.Count++;

                    var indent = new string(' ', Math.Max(0, _lists.Count - 1) * 4);

                    return list.IsOrdered
                        ? $"{indent}{list.Count}. "
                        : $"{indent}• ";

                default:
                    return null;
            }
        }

        private sealed class ListState
        {
            public bool IsOrdered { get; }

            public int Count { get; set; }

            public ListState(bool isOrdered)
            {
                IsOrdered = isOrdered;
            }
        }
    }
}
