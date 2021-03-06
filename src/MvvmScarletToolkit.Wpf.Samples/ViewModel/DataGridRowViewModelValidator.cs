using FluentValidation;

namespace MvvmScarletToolkit.Wpf.Samples
{
    public sealed class DataGridRowViewModelValidator : AbstractValidator<DataGridRowViewModel>
    {
        public DataGridRowViewModelValidator()
        {
            RuleFor(p => p.Color).NotEmpty();
        }
    }
}
