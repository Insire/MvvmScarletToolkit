using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Threading;

namespace MvvmScarletToolkit.Wpf.Samples.Features.DataGrid
{
    public sealed partial class DataGridViewModel : PagedSourceListViewModelBase<DataGridRowViewModel>
    {
        public GroupingViewModel Groups { get; }

        private Predicate<object>? _filter;
        public Predicate<object>? Filter
        {
            get => _filter;
            private set => SetProperty(ref _filter, value);
        }

        [ObservableProperty]
        private string _filterText;

        public DataGridViewModel(IScarletCommandBuilder commandBuilder, SynchronizationContext synchronizationContext)
            : base(commandBuilder, synchronizationContext, vm => vm.Name, new DataGridDataProvider(commandBuilder, 2000, 50))
        {
            _filterText = string.Empty;
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
