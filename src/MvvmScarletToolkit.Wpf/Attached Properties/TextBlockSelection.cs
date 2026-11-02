using Microsoft.Xaml.Behaviors;
using MvvmScarletToolkit.Wpf;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MvvmScarletToolkit
{
    /// <summary>
    /// Attached alternative to <see cref="TextBlockSelectionBehavior"/>, that can be set from a style.
    /// The appearance and the selected text are shared with the behavior, no matter how it was added.
    /// </summary>
    /// <example>
    /// In xaml
    /// <code>
    /// <![CDATA[
    /// <Style TargetType="{x:Type TextBlock}">
    ///     <Setter Property="mvvm:TextBlockSelection.IsEnabled" Value="True" />
    /// </Style>
    /// ]]>
    /// </code>
    /// </example>
    public static class TextBlockSelection
    {
        private static readonly DependencyPropertyKey SelectedTextPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
            "SelectedText",
            typeof(string),
            typeof(TextBlockSelection),
            new PropertyMetadata(string.Empty));

        /// <summary>Identifies the SelectedText attached property.</summary>
        public static readonly DependencyProperty SelectedTextProperty = SelectedTextPropertyKey.DependencyProperty;

        /// <summary>Identifies the IsEnabled attached property.</summary>
        public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.RegisterAttached(
            "IsEnabled",
            typeof(bool),
            typeof(TextBlockSelection),
            new PropertyMetadata(false, OnIsEnabledChanged));

        /// <summary>Identifies the SelectionBrush attached property.</summary>
        public static readonly DependencyProperty SelectionBrushProperty = DependencyProperty.RegisterAttached(
            "SelectionBrush",
            typeof(Brush),
            typeof(TextBlockSelection),
            new PropertyMetadata(SystemColors.HighlightBrush, OnSelectionAppearanceChanged));

        /// <summary>Identifies the SelectionOpacity attached property.</summary>
        public static readonly DependencyProperty SelectionOpacityProperty = DependencyProperty.RegisterAttached(
            "SelectionOpacity",
            typeof(double),
            typeof(TextBlockSelection),
            new PropertyMetadata(0.4d, OnSelectionAppearanceChanged));

        /// <summary>
        /// The currently selected text
        /// </summary>
        [AttachedPropertyBrowsableForType(typeof(TextBlock))]
        public static string GetSelectedText(TextBlock element)
        {
            return (string)element.GetValue(SelectedTextProperty);
        }

        internal static void SetSelectedText(TextBlock element, string value)
        {
            element.SetValue(SelectedTextPropertyKey, value);
        }

        /// <summary>
        /// Whether the text can be selected
        /// </summary>
        [AttachedPropertyBrowsableForType(typeof(TextBlock))]
        public static bool GetIsEnabled(TextBlock element)
        {
            return (bool)element.GetValue(IsEnabledProperty);
        }

        public static void SetIsEnabled(TextBlock element, bool value)
        {
            element.SetValue(IsEnabledProperty, value);
        }

        /// <summary>
        /// The brush that highlights the selected text
        /// </summary>
        [AttachedPropertyBrowsableForType(typeof(TextBlock))]
        public static Brush? GetSelectionBrush(TextBlock element)
        {
            return (Brush?)element.GetValue(SelectionBrushProperty);
        }

        public static void SetSelectionBrush(TextBlock element, Brush? value)
        {
            element.SetValue(SelectionBrushProperty, value);
        }

        /// <summary>
        /// The opacity of the selection brush. The highlight is drawn on top of the text, so it has to stay readable.
        /// </summary>
        [AttachedPropertyBrowsableForType(typeof(TextBlock))]
        public static double GetSelectionOpacity(TextBlock element)
        {
            return (double)element.GetValue(SelectionOpacityProperty);
        }

        public static void SetSelectionOpacity(TextBlock element, double value)
        {
            element.SetValue(SelectionOpacityProperty, value);
        }

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not TextBlock)
            {
                return;
            }

            var behaviors = Interaction.GetBehaviors(d);
            var behavior = behaviors.OfType<TextBlockSelectionBehavior>().FirstOrDefault();

            if (e.NewValue is true && behavior is null)
            {
                behaviors.Add(new TextBlockSelectionBehavior());
            }
            else if (e.NewValue is false && behavior is not null)
            {
                behaviors.Remove(behavior);
            }
        }

        private static void OnSelectionAppearanceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Interaction.GetBehaviors(d).OfType<TextBlockSelectionBehavior>().FirstOrDefault()?.InvalidateSelection();
        }
    }
}
