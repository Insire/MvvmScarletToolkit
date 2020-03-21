using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Controls;

namespace MvvmScarletToolkit
{
    [StyleTypedProperty(Property = nameof(WatermarkStyle), StyleTargetType = typeof(TextBox))]
    public sealed class WatermarkBehavior : Behavior<TextBox>
    {
        /// <summary>
        /// the style originally set on the textbox
        /// </summary>
        private Style? _cachedStyle;

        /// <summary>
        /// whether we want to reset the text inside the watermark property to string.empty
        /// </summary>
        private bool _skipTextReset;

        /// <summary>
        /// whether the text inside the Watermark property originated from us setting it inside this behavior
        /// </summary>
        private bool _isTextFromWatermark;

        /// <summary>
        /// The text to display, when the associated TextBox does not hold a value
        /// </summary>
        public string Watermark
        {
            get { return (string)GetValue(WatermarkProperty); }
            set { SetValue(WatermarkProperty, value); }
        }

        /// <summary>Identifies the <see cref="Watermark"/> dependency property.</summary>
        public static readonly DependencyProperty WatermarkProperty = DependencyProperty.Register(
            nameof(Watermark),
            typeof(string),
            typeof(WatermarkBehavior),
            new PropertyMetadata(default(string)));

        /// <summary>
        /// The Style to use for the associated TextBox, when it doesn't hold a value
        /// </summary>
        public Style WatermarkStyle
        {
            get { return (Style)GetValue(WatermarkStyleProperty); }
            set { SetValue(WatermarkStyleProperty, value); }
        }

        /// <summary>Identifies the <see cref="WatermarkStyle"/> dependency property.</summary>
        public static readonly DependencyProperty WatermarkStyleProperty = DependencyProperty.Register(
            nameof(WatermarkStyle),
            typeof(Style),
            typeof(WatermarkBehavior),
            new PropertyMetadata(default(Style)));

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.GotFocus += GotFocus;
            AssociatedObject.LostFocus += LostFocus;
            AssociatedObject.TextChanged += TextChanged;
            AssociatedObject.Loaded += Loaded;
        }

        private void Loaded(object sender, RoutedEventArgs e)
        {
            // set the initial style
            LostFocus(null, null);
        }

        private void TextChanged(object sender, TextChangedEventArgs e)
        {
            if (AssociatedObject.Text == string.Empty)
            {
                if (!_skipTextReset)
                {
                    _isTextFromWatermark = true;
                    AssociatedObject.SetCurrentValue(TextBox.TextProperty, Watermark);
                }

                SetStyle(false, WatermarkStyle);
            }
            else
            {
                SetStyle(false, _cachedStyle);
            }
        }

        private void LostFocus(object? sender, RoutedEventArgs? e)
        {
            if (AssociatedObject.Text == string.Empty)
            {
                _isTextFromWatermark = true;
                AssociatedObject.SetCurrentValue(TextBox.TextProperty, Watermark);

                SetStyle(true, WatermarkStyle);
            }
        }

        private void GotFocus(object sender, RoutedEventArgs e)
        {
            if (AssociatedObject.Text == Watermark)
            {
                if (_isTextFromWatermark)
                {
                    _skipTextReset = true;
                    AssociatedObject.SetCurrentValue(TextBox.TextProperty, string.Empty);
                    _skipTextReset = false;
                    _isTextFromWatermark = false;
                }

                SetStyle(true, _cachedStyle);
            }
        }

        private void SetStyle(bool setLocally, Style? style)
        {
            if (setLocally)
            {
                if (_cachedStyle != AssociatedObject.Style && AssociatedObject.Style != WatermarkStyle)
                {
                    _cachedStyle = AssociatedObject.Style;
                }

                AssociatedObject.SetCurrentValue(TextBox.StyleProperty, style);
            }
            else
            {
                AssociatedObject.SetCurrentValue(TextBox.StyleProperty, style);
            }
        }

        protected override void OnDetaching()
        {
            AssociatedObject.GotFocus -= GotFocus;
            AssociatedObject.LostFocus -= LostFocus;
            AssociatedObject.TextChanged -= TextChanged;
            AssociatedObject.Loaded -= Loaded;

            base.OnDetaching();
        }
    }
}
