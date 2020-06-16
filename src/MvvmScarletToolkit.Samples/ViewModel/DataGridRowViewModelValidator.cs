using FluentValidation;

namespace MvvmScarletToolkit.Samples
{
    public sealed class DataGridRowViewModelValidator : AbstractValidator<DataGridRowViewModel>
    {
        public DataGridRowViewModelValidator()
        {
            RuleFor(p => p.Color).NotEmpty();
        }
    }
}
