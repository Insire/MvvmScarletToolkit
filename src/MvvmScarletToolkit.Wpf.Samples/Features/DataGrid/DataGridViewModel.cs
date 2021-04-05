using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Wpf.Samples
{
    public sealed class DataGridDataProvider : IPagedDataProvider<DataGridRowViewModel>
    {
        private readonly IScarletCommandBuilder _scarletCommandBuilder;
        private readonly List<DataGridRowViewModel> _cache;

        public DataGridDataProvider(IScarletCommandBuilder scarletCommandBuilder, int pageCount, int pageSize)
        {
            _cache = new List<DataGridRowViewModel>(pageCount * pageSize);
            _scarletCommandBuilder = scarletCommandBuilder ?? throw new ArgumentNullException(nameof(scarletCommandBuilder));

            var page = 1;
            for (var i = 0; i < pageCount * pageSize; i++)
            {
                if (i % pageSize == 0)
                {
                    page++;
                }
                _cache.Add(new DataGridRowViewModel(_scarletCommandBuilder, page)
                {
                    Id = i,
                    CreatedOn = DateTime.Now,
                    Name = Guid.NewGuid().ToString(),
                    Color = $"#cc{i * 2:X2}{i * 3:X2}",
                });
            }
        }

        public Task<ICollection<DataGridRowViewModel>> Get(int index, int count, CancellationToken token)
        {
            return Task.FromResult<ICollection<DataGridRowViewModel>>(_cache.AsQueryable().TryPage(index, count).ToList());
        }
    }

    public sealed class DataGridViewModel : PagedSourceListViewModelBase<DataGridRowViewModel>
    {
        public GroupingViewModel Groups { get; }

        private Predicate<object> _filter;
        public Predicate<object> Filter
        {
            get { return _filter; }
            private set { SetProperty(ref _filter, value); }
        }

        private string _filterText;
        public string FilterText
        {
            get { return _filterText; }
            set { SetProperty(ref _filterText, value); }
        }

        public DataGridViewModel(IScarletCommandBuilder commandBuilder, SynchronizationContext synchronizationContext)
            : base(commandBuilder, synchronizationContext, vm => vm.Name, new DataGridDataProvider(commandBuilder, 2000, 50))
        {
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
