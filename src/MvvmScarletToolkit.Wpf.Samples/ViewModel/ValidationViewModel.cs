using FluentValidation;
using FluentValidation.Results;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Wpf.Samples
{
    public abstract class ValidationViewModel<T> : ObservableObject, INotifyDataErrorInfo
    {
        private readonly IValidator<T> _validator;
        private ValidationResult _result;

        public T ViewModel { get; }

        private bool _hasErrors;
        public bool HasErrors
        {
            get { return _hasErrors; }
            private set { SetProperty(ref _hasErrors, value); }
        }

        private bool _isValid;
        public bool IsValid
        {
            get { return _isValid; }
            private set { SetProperty(ref _isValid, value); }
        }

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        protected ValidationViewModel(IValidator<T> validator, T viewModel)
        {
            _validator = validator;
            ViewModel = viewModel;
        }

        public IEnumerable GetErrors(string propertyName)
        {
            return _result?.Errors.Where(p => p.PropertyName == propertyName).Select(p => p.ErrorMessage) ?? Enumerable.Empty<string>();
        }

        public async Task Validate()
        {
            _result = await _validator.ValidateAsync(ViewModel, _ => { }).ConfigureAwait(false);

            HasErrors = !_result.IsValid;
            IsValid = _result.IsValid;

            for (var i = 0; i < _result.Errors.Count; i++)
            {
                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(_result.Errors[i].PropertyName));
            }
        }
    }
}
