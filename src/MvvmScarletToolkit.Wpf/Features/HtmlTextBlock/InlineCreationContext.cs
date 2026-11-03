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
                    if ('/' + _activeStyle[i].Name == tag.Name)
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
                }
            }
        }
    }
}
