using System.Collections.Generic;
using System.Windows.Media;

namespace MvvmScarletToolkit.Wpf
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("PropertyChangedAnalyzers.PropertyChanged", "INPC001:The class has mutable properties and should implement INotifyPropertyChanged.", Justification = "does not support binding")]
    internal sealed class InlineCreationContext
    {
        private readonly List<HtmlTag> _activeStyle = new List<HtmlTag>();

        public bool Bold { get; private set; }
        public bool Italic { get; private set; }
        public bool Underline { get; private set; }
        public bool SubScript { get; private set; }
        public bool SuperScript { get; private set; }
        public string HyperLink { get; private set; }
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
                }
            }
        }
    }
}
