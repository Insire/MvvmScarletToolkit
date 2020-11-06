using System;

namespace MvvmScarletToolkit.Samples
{
    public sealed class DataGridRowValidationViewModel : ValidationViewModel<DataGridRowViewModel>
    {
        public string Name => ViewModel.Name;

        public string Color => ViewModel.Color;

        public DateTime CreatedOn => ViewModel.CreatedOn;

        public DateTime UpdatedOn => ViewModel.UpdatedOn;

        public bool IsSelected => ViewModel.IsSelected;

        public DataGridRowValidationViewModel(DataGridRowViewModelValidator validator, DataGridRowViewModel viewModel)
            : base(validator, viewModel)
        {
        }
    }
}
