using CommunityToolkit.Mvvm.ComponentModel;
using System.Globalization;

namespace MvvmScarletToolkit.Wpf.Samples.Features.DataGrid
{
    public sealed partial class DataGridViewModel : PagedSourceListViewModelBase<DataGridRowViewModel>
    {
        public GroupingViewModel Groups { get; }

        public DataGridFilterViewModel ColumnFilters { get; }

        public CsvExporter<DataGridRowViewModel> CsvExporter { get; }

        /// <summary>
        /// Stored by the tracker configured in <see cref="App"/>
        /// </summary>
        [ObservableProperty]
        public partial DataGridLayout? Layout { get; set; }

        [ObservableProperty]
        public partial string FilterText { get; set; }

        partial void OnFilterTextChanged(string value)
        {
            ColumnFilters?.Refresh();
        }

        public DataGridViewModel(IScarletCommandBuilder commandBuilder, SynchronizationContext synchronizationContext, ExportFileDialog exportFileDialog)
            // the key has to be immutable, Name can be edited in the details view
            : base(commandBuilder, synchronizationContext, vm => vm.Id.ToString(CultureInfo.InvariantCulture), new DataGridDataProvider(commandBuilder, 2000, 50))
        {
            FilterText = string.Empty;
            Groups = GroupingViewModel.Create(commandBuilder, Items, new GroupingViewModelOptions { ExcludeToolkitProperties = true });
            ColumnFilters = DataGridFilterViewModel.Create<DataGridRowViewModel>(commandBuilder);
            ColumnFilters.AdditionalFilter = IsMatch;
            CsvExporter = new CsvExporter<DataGridRowViewModel>(commandBuilder, exportFileDialog, new Dictionary<string, Func<DataGridRowViewModel, object?>>
            {
                [nameof(DataGridRowViewModel.IsSelected)] = p => p.IsSelected,
                [nameof(DataGridRowViewModel.Id)] = p => p.Id,
                [nameof(DataGridRowViewModel.Name)] = p => p.Name,
                [nameof(DataGridRowViewModel.Color)] = p => p.Color,
                [nameof(DataGridRowViewModel.CreatedOn)] = p => p.CreatedOn,
                [nameof(DataGridRowViewModel.UpdatedOn)] = p => p.UpdatedOn,
            });

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
                // the rows are disposed by the base class, when they are removed from Items
                Groups.Dispose();
                ColumnFilters.Dispose();
                CsvExporter.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
