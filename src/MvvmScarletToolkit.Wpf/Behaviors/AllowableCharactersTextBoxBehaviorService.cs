using System.Text.RegularExpressions;

namespace MvvmScarletToolkit
{
    public static class AllowableCharactersTextBoxBehaviorService
    {
        public static bool IsValid(string? newText, bool isPaste, int selectedTextLength, int maxLength, string text, int caretIndex, string regex, bool isInsertKeyToggled)
        {
            return !ExceedsMaxLength(newText, isPaste, selectedTextLength, maxLength, text, caretIndex, isInsertKeyToggled)
                && Regex.IsMatch(newText ?? string.Empty, regex, RegexOptions.CultureInvariant);
        }

        private static bool ExceedsMaxLength(string? newText, bool isPaste, int selectedTextLength, int maxLength, string text, int caretIndex, bool isInsertKeyToggled)
        {
            if (maxLength == 0)
                return false;

            return LengthOfModifiedText(newText, isPaste, selectedTextLength, text, caretIndex, isInsertKeyToggled) > maxLength;
        }

        private static int LengthOfModifiedText(string? newText, bool isPaste, int selectedTextLength, string text, int caretIndex, bool isInsertKeyToggled)
        {
            var newTextLength = newText?.Length ?? 0;

            switch (selectedTextLength)
            {
                // should we replace text?
                case <= 0 when !isPaste && !isInsertKeyToggled:
                    return text.Length + newTextLength;

                case > 0:
                    text = text.Remove(caretIndex, selectedTextLength);
                    break;

                default:
                {
                    var newLength = text.Length < newTextLength
                        ? text.Length
                        : newTextLength;

                    if (caretIndex < text.Length && caretIndex + newLength <= text.Length)
                    {
                        text = text.Remove(caretIndex, newLength);
                    }

                    break;
                }
            }

            // insert or append
            return text.Length + newTextLength;
        }
    }
}
