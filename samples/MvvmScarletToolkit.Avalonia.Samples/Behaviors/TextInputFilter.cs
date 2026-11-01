using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using System.Text.RegularExpressions;

namespace MvvmScarletToolkit.Avalonia.Samples.Behaviors
{
    /// <summary>
    /// Rejects typed and pasted text, the counterpart of the AllowableCharactersTextBoxBehavior in the WPF package.
    /// The maximum length is not part of this, <see cref="TextBox.MaxLength"/> covers that.
    /// </summary>
    public sealed class TextInputFilter
    {
        public static readonly AttachedProperty<string?> RegularExpressionProperty = AvaloniaProperty.RegisterAttached<TextInputFilter, TextBox, string?>("RegularExpression");

        public static readonly AttachedProperty<bool> AllowWhitespaceProperty = AvaloniaProperty.RegisterAttached<TextInputFilter, TextBox, bool>("AllowWhitespace", defaultValue: true);

        static TextInputFilter()
        {
            InputElement.TextInputEvent.AddClassHandler<TextBox>(OnTextInput, RoutingStrategies.Tunnel);
            TextBox.PastingFromClipboardEvent.AddClassHandler<TextBox>(OnPaste, RoutingStrategies.Bubble);
        }

        private TextInputFilter()
        {
        }

        public static string? GetRegularExpression(TextBox textBox) => textBox.GetValue(RegularExpressionProperty);

        public static void SetRegularExpression(TextBox textBox, string? value) => textBox.SetValue(RegularExpressionProperty, value);

        public static bool GetAllowWhitespace(TextBox textBox) => textBox.GetValue(AllowWhitespaceProperty);

        public static void SetAllowWhitespace(TextBox textBox, bool value) => textBox.SetValue(AllowWhitespaceProperty, value);

        private static bool IsFiltered(TextBox textBox)
        {
            return GetRegularExpression(textBox) is not null || !GetAllowWhitespace(textBox);
        }

        private static void OnTextInput(TextBox textBox, TextInputEventArgs e)
        {
            if (IsFiltered(textBox) && !IsValid(textBox, e.Text))
            {
                e.Handled = true;
            }
        }

        private static async void OnPaste(TextBox textBox, RoutedEventArgs e)
        {
            if (!IsFiltered(textBox) || TopLevel.GetTopLevel(textBox)?.Clipboard is not { } clipboard)
            {
                return;
            }

            // the clipboard can only be read asynchronously, so pasting is taken over completely
            e.Handled = true;

            var text = await clipboard.TryGetTextAsync();
            if (text is not null && IsValid(textBox, text))
            {
                textBox.SelectedText = text;
            }
        }

        private static bool IsValid(TextBox textBox, string? input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return true;
            }

            if (!GetAllowWhitespace(textBox) && input.Any(char.IsWhiteSpace))
            {
                return false;
            }

            var pattern = GetRegularExpression(textBox);
            if (string.IsNullOrEmpty(pattern))
            {
                return true;
            }

            try
            {
                return Regex.IsMatch(GetResultingText(textBox, input), pattern);
            }
            catch (ArgumentException)
            {
                // the pattern is user input in the sample and may be incomplete while it is being typed
                return true;
            }
        }

        private static string GetResultingText(TextBox textBox, string input)
        {
            var text = textBox.Text ?? string.Empty;
            var start = Math.Min(textBox.SelectionStart, textBox.SelectionEnd);
            var end = Math.Max(textBox.SelectionStart, textBox.SelectionEnd);

            return string.Concat(text.AsSpan(0, start), input, text.AsSpan(end));
        }
    }
}
