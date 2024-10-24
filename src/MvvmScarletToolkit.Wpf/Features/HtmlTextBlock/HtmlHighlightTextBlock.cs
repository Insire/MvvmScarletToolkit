using System;
using System.Windows;

namespace MvvmScarletToolkit.Wpf
{
    public class HtmlHighlightTextBlock : HtmlTextBlock
    {
        /// <summary>Identifies the <see cref="Highlight"/> dependency property.</summary>
        public static readonly DependencyProperty HighlightProperty = DependencyProperty.Register(
            nameof(Highlight),
            typeof(string),
            typeof(HtmlHighlightTextBlock),
            new UIPropertyMetadata(string.Empty));

        public string Highlight
        {
            get => (string)GetValue(HighlightProperty);
            set => SetValue(HighlightProperty, value);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            Parse(Html);
        }

        protected override void Parse(string html)
        {
            Inlines.Clear();

            if (!string.IsNullOrEmpty(Highlight))
            {
                var index = html.IndexOf(Highlight, StringComparison.InvariantCultureIgnoreCase);
                while (index != -1)
                {
                    html = string.Format("{0}[b]{1}[/b]{2}", html.Substring(0, index), html.Substring(index, Highlight.Length), html.Substring(index + Highlight.Length));
                    index = html.IndexOf(Highlight, index + 7 + Highlight.Length, StringComparison.InvariantCultureIgnoreCase);
                }
            }

            this.UpdateWith(html);
        }
    }
}
