using CommunityToolkit.Mvvm.ComponentModel;
using MvvmScarletToolkit.Observables;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Input;

namespace MvvmScarletToolkit
{
    /// <summary>
    /// The filter of a single column. <see cref="Operators"/> only offers what makes sense for the type of the property:
    /// comparisons for numbers, dates and times, equality for bool, enum and <see cref="Guid"/>, and text matching for anything else.
    /// A <see cref="Guid"/> can be matched as text as well, to find it by a part of its value.
    /// <see cref="Operator"/> and <see cref="Value"/> are the input of the user and don't change the rows until <see cref="Apply"/> is called.
    /// </summary>
    public sealed partial class ColumnFilterViewModel : ViewModelBase
    {
        private static readonly HashSet<Type> _orderedTypes =
        [
            typeof(sbyte),
            typeof(byte),
            typeof(short),
            typeof(ushort),
            typeof(int),
            typeof(uint),
            typeof(long),
            typeof(ulong),
            typeof(float),
            typeof(double),
            typeof(decimal),
            typeof(DateTime),
            typeof(DateTimeOffset),
            typeof(DateOnly),
            typeof(TimeOnly),
            typeof(TimeSpan),
        ];

        private static readonly ColumnFilterOperator[] _orderedOperators =
        [
            ColumnFilterOperator.Equals,
            ColumnFilterOperator.NotEquals,
            ColumnFilterOperator.GreaterThan,
            ColumnFilterOperator.GreaterThanOrEqual,
            ColumnFilterOperator.LessThan,
            ColumnFilterOperator.LessThanOrEqual,
        ];

        private static readonly ColumnFilterOperator[] _equalityOperators = [ColumnFilterOperator.Equals, ColumnFilterOperator.NotEquals];

        private static readonly ColumnFilterOperator[] _textOperators = [ColumnFilterOperator.Contains, ColumnFilterOperator.Equals, ColumnFilterOperator.NotEquals];

        private readonly PropertyAccessor _accessor;
        private readonly Type _type;

        // what the current input evaluates to, null when it can't be applied
        private object? _candidate;

        // what the rows are filtered by
        private ColumnFilterOperator _appliedOperator;
        private string? _appliedText;
        private object? _appliedValue;

        public string PropertyPath { get; }

        public IReadOnlyList<ColumnFilterOperator> Operators { get; }

        [ObservableProperty]
        public partial ColumnFilterOperator Operator { get; set; }

        /// <summary>
        /// The text entered by the user. An empty text disables this filter.
        /// </summary>
        [ObservableProperty]
        public partial string? Value { get; set; }

        /// <summary>
        /// Whether <see cref="Value"/> failed validation. This filter is disabled until that is fixed.
        /// </summary>
        [ObservableProperty]
        public partial bool HasError { get; private set; }

        /// <summary>
        /// Why <see cref="Value"/> failed validation
        /// </summary>
        [ObservableProperty]
        public partial string? Error { get; private set; }

        /// <summary>
        /// Whether <see cref="Value"/> is validated. When disabled, there are no errors and a value that can't be converted to the type of the property is compared by its text.
        /// </summary>
        [ObservableProperty]
        public partial bool IsValidationEnabled { get; set; }

        /// <summary>
        /// Whether this filter currently limits the rows
        /// </summary>
        [ObservableProperty]
        public partial bool IsApplied { get; private set; }

        /// <summary>
        /// Additional rules for free text, checked after the value has been converted to the type of the property. <see cref="Options"/> are not checked.
        /// </summary>
        public ObservableCollection<ValidationRule> ValidationRules { get; }

        /// <summary>
        /// The possible values of a bool or enum property, null for any other type
        /// </summary>
        public IReadOnlyList<string>? Options { get; }

        /// <summary>
        /// Clears <see cref="Value"/>. Like any other input, this takes effect with <see cref="Apply"/>.
        /// </summary>
        public ICommand ClearCommand { get; }

        internal event EventHandler? Applied;

        internal ColumnFilterViewModel(IScarletCommandBuilder commandBuilder, string propertyPath, PropertyAccessor accessor)
            : base(commandBuilder)
        {
            PropertyPath = propertyPath;
            _accessor = accessor;
            _type = Nullable.GetUnderlyingType(accessor.PropertyType) ?? accessor.PropertyType;

            ValidationRules = [];
            ValidationRules.CollectionChanged += (_, _) => Validate();

            Operators = GetOperators(_type);
            Operator = Operators[0];
            IsValidationEnabled = true;

            if (_type.IsEnum)
            {
                Options = Enum.GetNames(_type);
            }
            else if (_type == typeof(bool))
            {
                Options = [bool.TrueString, bool.FalseString];
            }

            ClearCommand = commandBuilder
                .Create(Clear, CanClear)
                .WithSingleExecution()
                .WithBusyNotification(BusyStack)
                .Build();
        }

        /// <summary>
        /// Filters the rows by the current <see cref="Operator"/> and <see cref="Value"/>
        /// </summary>
        public void Apply()
        {
            if (Commit())
            {
                Applied?.Invoke(this, EventArgs.Empty);
            }
        }

        partial void OnValueChanged(string? value)
        {
            Validate();
        }

        partial void OnOperatorChanged(ColumnFilterOperator value)
        {
            // Contains matches text, the same value may not be valid for the other operators
            Validate();
        }

        partial void OnIsValidationEnabledChanged(bool value)
        {
            Validate();
        }

        /// <returns>whether the rows have to be filtered again</returns>
        internal bool Commit()
        {
            var isApplied = _candidate is not null;
            var isChanged = isApplied != IsApplied
                || (isApplied && (_appliedOperator != Operator || _appliedText != Value || !Equals(_appliedValue, _candidate)));

            _appliedOperator = Operator;
            _appliedText = Value;
            _appliedValue = _candidate;
            IsApplied = isApplied;

            return isChanged;
        }

        internal bool IsMatch(object item)
        {
            if (!IsApplied)
            {
                return true;
            }

            var value = _accessor.GetValue(item);

            return _appliedOperator switch
            {
                ColumnFilterOperator.Contains => value?.ToString()?.IndexOf(_appliedText!, StringComparison.CurrentCultureIgnoreCase) >= 0,
                ColumnFilterOperator.Equals => Compare(value) == 0,
                ColumnFilterOperator.NotEquals => Compare(value) != 0,
                ColumnFilterOperator.GreaterThan => value is not null && Compare(value) > 0,
                ColumnFilterOperator.GreaterThanOrEqual => value is not null && Compare(value) >= 0,
                ColumnFilterOperator.LessThan => value is not null && Compare(value) < 0,
                ColumnFilterOperator.LessThanOrEqual => value is not null && Compare(value) <= 0,
                _ => true,
            };
        }

        private int Compare(object? value)
        {
            if (value is null)
            {
                return -1;
            }

            return _appliedValue is string text
                ? string.Compare(value.ToString(), text, StringComparison.CurrentCultureIgnoreCase)
                : ((IComparable)value).CompareTo(_appliedValue);
        }

        private void Validate()
        {
            _candidate = null;
            Error = null;
            HasError = false;

            if (string.IsNullOrEmpty(Value))
            {
                return;
            }

            _candidate = Value;

            if (Operator != ColumnFilterOperator.Contains && IsTyped(_type))
            {
                if (TryConvert(Value, out var converted))
                {
                    _candidate = converted;
                }
                else if (IsValidationEnabled)
                {
                    Fail($"'{Value}' is not a valid {_type.Name}.");
                    return;
                }
            }

            if (!IsValidationEnabled || Options is not null)
            {
                return;
            }

            foreach (var rule in ValidationRules)
            {
                var result = rule.Validate(Value, CultureInfo.CurrentCulture);
                if (!result.IsValid)
                {
                    Fail(result.ErrorContent?.ToString() ?? $"'{Value}' is not valid.");
                    return;
                }
            }
        }

        private void Fail(string error)
        {
            _candidate = null;
            Error = error;
            HasError = true;
        }

        private bool TryConvert(string value, out object? result)
        {
            try
            {
                result = _type.IsEnum
                    ? Enum.Parse(_type, value, true)
                    : TypeDescriptor.GetConverter(_type).ConvertFromString(null, CultureInfo.CurrentCulture, value);

                return result is not null;
            }
            catch (Exception ex) when (ex is FormatException or OverflowException or ArgumentException or NotSupportedException)
            {
                result = null;
                return false;
            }
        }

        private Task Clear()
        {
            return Dispatcher.Invoke(() => Value = null);
        }

        private bool CanClear()
        {
            return !IsDisposed
                && !string.IsNullOrEmpty(Value);
        }

        private static ColumnFilterOperator[] GetOperators(Type type)
        {
            if (_orderedTypes.Contains(type))
            {
                return _orderedOperators;
            }

            if (type == typeof(bool) || type.IsEnum)
            {
                return _equalityOperators;
            }

            return _textOperators;
        }

        /// <summary>
        /// Whether values are compared as <paramref name="type"/>, anything else is compared by its text
        /// </summary>
        private static bool IsTyped(Type type)
        {
            return _orderedTypes.Contains(type) || type == typeof(bool) || type == typeof(Guid) || type.IsEnum;
        }
    }
}
