using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;

namespace MvvmScarletToolkit
{
    /// <summary>
    /// Displays <see cref="Watermark"/> on top of the associated TextBox while it is empty and not focused.
    /// The watermark is drawn in an adorner, so <see cref="TextBox.Text"/> and with it any binding never receives the watermark text.
    /// </summary>
    [StyleTypedProperty(Property = nameof(WatermarkStyle), StyleTargetType = typeof(TextBox))]
    public sealed class WatermarkBehavior : Behavior<TextBox>
    {
        /// <summary>
        /// the style originally set on the textbox
        /// </summary>
        private Style? _cachedStyle;

        private WatermarkAdorner? _adorner;

        /// <summary>
        /// The text to display, when the associated TextBox does not hold a value
        /// </summary>
        public string? Watermark
        {
            get => (string?)GetValue(WatermarkProperty);
            set => SetValue(WatermarkProperty, value);
        }

        /// <summary>Identifies the <see cref="Watermark"/> dependency property.</summary>
        public static readonly DependencyProperty WatermarkProperty = DependencyProperty.Register(
            nameof(Watermark),
            typeof(string),
            typeof(WatermarkBehavior),
            new PropertyMetadata(default(string), OnPropertyChanged));

        /// <summary>
        /// The Style to use for the associated TextBox, when it doesn't hold a value
        /// </summary>
        public Style? WatermarkStyle
        {
            get => (Style?)GetValue(WatermarkStyleProperty);
            set => SetValue(WatermarkStyleProperty, value);
        }

        /// <summary>Identifies the <see cref="WatermarkStyle"/> dependency property.</summary>
        public static readonly DependencyProperty WatermarkStyleProperty = DependencyProperty.Register(
            nameof(WatermarkStyle),
            typeof(Style),
            typeof(WatermarkBehavior),
            new PropertyMetadata(default(Style), OnPropertyChanged));

        protected override void OnAttached()
        {
            base.OnAttached();

            _cachedStyle = AssociatedObject.Style;

            AssociatedObject.GotKeyboardFocus += FocusChanged;
            AssociatedObject.LostKeyboardFocus += FocusChanged;
            AssociatedObject.TextChanged += TextChanged;
            AssociatedObject.Loaded += Loaded;
            AssociatedObject.Unloaded += Unloaded;

            if (AssociatedObject.IsLoaded)
            {
                Loaded(AssociatedObject, new RoutedEventArgs());
            }
        }

        protected override void OnDetaching()
        {
            AssociatedObject.GotKeyboardFocus -= FocusChanged;
            AssociatedObject.LostKeyboardFocus -= FocusChanged;
            AssociatedObject.TextChanged -= TextChanged;
            AssociatedObject.Loaded -= Loaded;
            AssociatedObject.Unloaded -= Unloaded;

            RemoveAdorner();

            if (WatermarkStyle is not null && ReferenceEquals(AssociatedObject.Style, WatermarkStyle))
            {
                AssociatedObject.SetValue(FrameworkElement.StyleProperty, _cachedStyle);
            }

            base.OnDetaching();
        }

        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WatermarkBehavior behavior && behavior.AssociatedObject is not null)
            {
                behavior.Update();
            }
        }

        private void Loaded(object sender, RoutedEventArgs e)
        {
            if (_adorner is null)
            {
                // the adorner layer is only available once the textbox is part of a visual tree
                var layer = AdornerLayer.GetAdornerLayer(AssociatedObject);
                if (layer is not null)
                {
                    _adorner = new WatermarkAdorner(AssociatedObject);
                    layer.Add(_adorner);
                }
            }

            Update();
        }

        private void Unloaded(object sender, RoutedEventArgs e)
        {
            RemoveAdorner();
        }

        private void TextChanged(object sender, TextChangedEventArgs e)
        {
            Update();
        }

        private void FocusChanged(object sender, RoutedEventArgs e)
        {
            Update();
        }

        private void Update()
        {
            var showWatermark = string.IsNullOrEmpty(AssociatedObject.Text) && !AssociatedObject.IsKeyboardFocusWithin;

            if (_adorner is not null)
            {
                _adorner.Text = Watermark;
                _adorner.Visibility = showWatermark ? Visibility.Visible : Visibility.Collapsed;
            }

            if (WatermarkStyle is null)
            {
                return;
            }

            var style = showWatermark ? WatermarkStyle : _cachedStyle;
            if (!ReferenceEquals(AssociatedObject.Style, style))
            {
                AssociatedObject.SetValue(FrameworkElement.StyleProperty, style);
            }
        }

        private void RemoveAdorner()
        {
            if (_adorner is null)
            {
                return;
            }

            AdornerLayer.GetAdornerLayer(AssociatedObject)?.Remove(_adorner);
            _adorner = null;
        }

        private sealed class WatermarkAdorner : Adorner
        {
            private readonly TextBlock _textBlock;

            public string? Text
            {
                set => _textBlock.Text = value;
            }

            public WatermarkAdorner(TextBox textBox)
                : base(textBox)
            {
                IsHitTestVisible = false;

                _textBlock = new TextBlock
                {
                    Opacity = 0.6,
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    // roughly where the TextBox renders its own text
                    Margin = new Thickness(
                        textBox.BorderThickness.Left + textBox.Padding.Left + 2,
                        textBox.BorderThickness.Top + textBox.Padding.Top,
                        textBox.BorderThickness.Right + textBox.Padding.Right + 2,
                        textBox.BorderThickness.Bottom + textBox.Padding.Bottom),
                };

                // the adorner layer does not inherit these from the textbox
                Bind(TextElement.FontFamilyProperty, nameof(TextBox.FontFamily));
                Bind(TextElement.FontSizeProperty, nameof(TextBox.FontSize));
                Bind(TextElement.FontStyleProperty, nameof(TextBox.FontStyle));
                Bind(TextElement.FontWeightProperty, nameof(TextBox.FontWeight));
                Bind(TextElement.ForegroundProperty, nameof(TextBox.Foreground));
                Bind(VerticalAlignmentProperty, nameof(TextBox.VerticalContentAlignment));

                AddVisualChild(_textBlock);

                void Bind(DependencyProperty property, string path)
                {
                    _textBlock.SetBinding(property, new Binding(path) { Source = textBox });
                }
            }

            protected override int VisualChildrenCount => 1;

            protected override Visual GetVisualChild(int index) => _textBlock;

            protected override Size MeasureOverride(Size constraint)
            {
                _textBlock.Measure(AdornedElement.RenderSize);
                return AdornedElement.RenderSize;
            }

            protected override Size ArrangeOverride(Size finalSize)
            {
                _textBlock.Arrange(new Rect(finalSize));
                return finalSize;
            }
        }
    }
}
