using System.Globalization;
using System.Windows.Controls;

namespace MvvmScarletToolkit.Wpf.Samples.Features.DataGrid
{
    /// <summary>
    /// An example for a custom rule of a text filter
    /// </summary>
    public sealed class MinimumLengthValidationRule : ValidationRule
    {
        private readonly int _length;

        public MinimumLengthValidationRule(int length)
        {
            _length = length;
        }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            return value is string { Length: var length } && length >= _length
                ? ValidationResult.ValidResult
                : new ValidationResult(false, $"Enter at least {_length} characters.");
        }
    }
}
