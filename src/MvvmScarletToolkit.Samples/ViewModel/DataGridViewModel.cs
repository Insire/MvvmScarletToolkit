using MvvmScarletToolkit.Observables;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MvvmScarletToolkit.Samples
{
    public class DataGridViewModel : BusinessViewModelListBase<DataGridRowViewModel>
    {
        public GroupingViewModel Groups { get; }

        private Predicate<object> _filter;
        public Predicate<object> Filter
        {
            get { return _filter; }
            private set { SetValue(ref _filter, value); }
        }

        private string _filterText;
        public string FilterText
        {
            get { return _filterText; }
            set { SetValue(ref _filterText, value, onChanged: () => OnFilterTextChanged(value)); }
        }

        public DataGridViewModel(IScarletCommandBuilder commandBuilder)
            : base(commandBuilder)
        {
            Groups = new GroupingViewModel(commandBuilder, () => CollectionViewSource.GetDefaultView(Items), typeof(DataGridRowViewModel));
        }

        protected override async Task RefreshInternal(CancellationToken token)
        {
            for (var i = 0; i < 50; i++)
            {
                await Add(new DataGridRowViewModel(CommandBuilder)
                {
                    CreatedOn = DateTime.Now,
                    Name = Guid.NewGuid().ToString(),
                    Color = $"#cc{i * 2:X2}{i * 3:X2}",
                });
            }
        }

        private void OnFilterTextChanged(string value)
        {
            Filter = string.IsNullOrEmpty(value) ? default(Predicate<object>) : IsMatch;
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
    }
}
