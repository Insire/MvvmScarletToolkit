using CommunityToolkit.Mvvm.ComponentModel;

namespace MvvmScarletToolkit.Wpf.Samples.Features.DataGrid
{
    public sealed partial class DataGridViewModel : PagedSourceListViewModelBase<DataGridRowViewModel>
    {
        public GroupingViewModel Groups { get; }

        [ObservableProperty]
        public partial Predicate<object>? Filter { get; private set; }

        [ObservableProperty]
        public partial string FilterText { get; set; }

        public DataGridViewModel(IScarletCommandBuilder commandBuilder, SynchronizationContext synchronizationContext)
            : base(commandBuilder, synchronizationContext, vm => vm.Name, new DataGridDataProvider(commandBuilder, 2000, 50))
        {
            FilterText = string.Empty;
            Groups = GroupingViewModel.Create(Items);
            Filter = IsMatch;

            PageSize = 50;
            TotalPageCount = 2000;
            CurrentPage = 1;
        }

        private bool IsMatch(object item)
        {
            if (item is DataGridRowViewModel viewmodel)
            {
                return IsMatch(viewmodel, FilterText);
            }
            else
            {
                return false;
            }
        }

        private static bool IsMatch(DataGridRowViewModel viewModel, string filterText)
        {
            if (string.IsNullOrEmpty(filterText))
            {
                return true;
            }

            var name = viewModel.Name;
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }

            if (filterText.Length == 1)
            {
                return name.StartsWith(filterText, StringComparison.OrdinalIgnoreCase);
            }

            return name.IndexOf(filterText, 0, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Groups.Dispose();

                Items.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
