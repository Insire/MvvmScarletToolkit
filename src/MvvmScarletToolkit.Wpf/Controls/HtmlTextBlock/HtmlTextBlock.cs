using System.Windows;
using System.Windows.Controls;

namespace MvvmScarletToolkit.Wpf
{
    public class HtmlTextBlock : TextBlock
    {
        public static DependencyProperty HtmlProperty = DependencyProperty.Register(
            nameof(Html),
            typeof(string),
            typeof(HtmlTextBlock),
            new PropertyMetadata(string.Empty, new PropertyChangedCallback(OnHtmlChanged)));

        public string Html
        {
            get { return (string)GetValue(HtmlProperty); }
            set { SetValue(HtmlProperty, value); }
        }

        /// <summary>When overridden in a derived class, is invoked whenever application code or internal processes call <see cref="M:System.Windows.FrameworkElement.ApplyTemplate" />.</summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            Parse(Html);
        }

        protected virtual void Parse(string html)
        {
            Inlines.Clear();
            if (string.IsNullOrEmpty(html))
            {
                return;
            }

            this.UpdateWith(html);
        }

        private static void OnHtmlChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            if (o is HtmlTextBlock textBlock && e.NewValue is string text)
            {
                textBlock.Parse(text);
            }
        }
    }
}
