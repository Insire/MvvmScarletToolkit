using CommunityToolkit.Mvvm.ComponentModel;
using MvvmScarletToolkit.Observables;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MvvmScarletToolkit
{
    /// <summary>
    /// Filters rows by one <see cref="ColumnFilterViewModel"/> per column, combined with AND.
    /// Use it with <see cref="Wpf.DataGridFilterBehavior"/>, or bind <see cref="Predicate"/> to <see cref="Filter.ByProperty"/> and <see cref="Version"/> to <see cref="Filter.RefreshWhenChangedProperty"/>.
    /// </summary>
    public sealed partial class DataGridFilterViewModel : ViewModelBase
    {
        public static DataGridFilterViewModel Create<T>(IScarletCommandBuilder commandBuilder)
            where T : class
        {
            return new DataGridFilterViewModel(commandBuilder, typeof(T));
        }

        private readonly Type _itemType;
        private readonly ObservableCollection<ColumnFilterViewModel> _filters;

        private ColumnFilterViewModel[] _enabled;

        public ReadOnlyObservableCollection<ColumnFilterViewModel> Filters { get; }

        /// <summary>
        /// The predicate combining all filters
        /// </summary>
        public Predicate<object> Predicate { get; }

        /// <summary>
        /// Changes every time the result of <see cref="Predicate"/> might have changed
        /// </summary>
        [ObservableProperty]
        public partial int Version { get; private set; }

        /// <summary>
        /// An optional filter of the application, that has to match as well
        /// </summary>
        public Predicate<object>? AdditionalFilter
        {
            get;
            set
            {
                if (SetProperty(ref field, value))
                {
                    Version++;
                }
            }
        }

        /// <summary>
        /// Enables or disables <see cref="ColumnFilterViewModel.IsValidationEnabled"/> for all columns
        /// </summary>
        public bool IsValidationEnabled
        {
            get;
            set
            {
                if (SetProperty(ref field, value))
                {
                    foreach (var filter in _filters)
                    {
                        filter.IsValidationEnabled = value;
                    }
                }
            }
        } = true;

        public ICommand ClearCommand { get; }

        public DataGridFilterViewModel(IScarletCommandBuilder commandBuilder, Type itemType)
            : base(commandBuilder)
        {
            _itemType = itemType ?? throw new ArgumentNullException(nameof(itemType));
            _filters = [];
            _enabled = [];

            Filters = new ReadOnlyObservableCollection<ColumnFilterViewModel>(_filters);
            Predicate = IsMatch;

            ClearCommand = commandBuilder
                .Create(Clear, CanClear)
                .WithSingleExecution()
                .WithBusyNotification(BusyStack)
                .Build();
        }

        /// <summary>
        /// Provides the filter for <paramref name="propertyPath"/>
        /// </summary>
        /// <returns>null when the path can't be read, like for indexers</returns>
        public ColumnFilterViewModel? GetFilter(string propertyPath)
        {
            ArgumentException.ThrowIfNullOrEmpty(propertyPath);

            var filter = _filters.FirstOrDefault(p => p.PropertyPath == propertyPath);
            if (filter is not null)
            {
                return filter;
            }

            var accessor = PropertyAccessor.Get(_itemType, propertyPath);
            if (accessor is null)
            {
                return null;
            }

            filter = new ColumnFilterViewModel(CommandBuilder, propertyPath, accessor)
            {
                IsValidationEnabled = IsValidationEnabled,
            };
            filter.Applied += OnFilterApplied;
            _filters.Add(filter);

            return filter;
        }

        /// <summary>
        /// Applies the filters again, for example when something <see cref="AdditionalFilter"/> depends on has changed
        /// </summary>
        public void Refresh()
        {
            Version++;
        }

        private bool IsMatch(object item)
        {
            if (AdditionalFilter?.Invoke(item) == false)
            {
                return false;
            }

            foreach (var filter in _enabled)
            {
                if (!filter.IsMatch(item))
                {
                    return false;
                }
            }

            return true;
        }

        private void OnFilterApplied(object? sender, EventArgs e)
        {
            Update();
        }

        private void Update()
        {
            _enabled = _filters.Where(p => p.IsApplied).ToArray();
            Version++;
        }

        private Task Clear()
        {
            return Dispatcher.Invoke(() =>
            {
                // only once, filtering all rows is expensive
                var isChanged = false;
                foreach (var filter in _filters)
                {
                    filter.Value = null;
                    isChanged |= filter.Commit();
                }

                if (isChanged)
                {
                    Update();
                }
            });
        }

        private bool CanClear()
        {
            return !IsDisposed
                && _filters.Any(p => !string.IsNullOrEmpty(p.Value));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var filter in _filters)
                {
                    filter.Applied -= OnFilterApplied;
                    filter.Dispose();
                }
            }

            base.Dispose(disposing);
        }
    }
}
