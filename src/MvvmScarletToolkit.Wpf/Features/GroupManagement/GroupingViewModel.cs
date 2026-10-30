using CommunityToolkit.Mvvm.Messaging;
using MvvmScarletToolkit.Observables;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Data;
using System.Windows.Input;

namespace MvvmScarletToolkit
{
    // manages
    // https://github.com/tom-englert/DataGridExtensions
    // http://dotnetpattern.com/wpf-datagrid-grouping
    public sealed class GroupingViewModel : BusinessViewModelListBase<GroupsViewModel>
    {
        public static GroupingViewModel Create<T>(IEnumerable<T> collection)
            where T : class, INotifyPropertyChanged
        {
            return Create(ScarletCommandBuilder.Default, collection);
        }

        public static GroupingViewModel Create<T>(IScarletCommandBuilder commandBuilder, IEnumerable<T> collection)
            where T : class, INotifyPropertyChanged
        {
            return Create(commandBuilder, collection, null);
        }

        public static GroupingViewModel Create<T>(IScarletCommandBuilder commandBuilder, IEnumerable<T> collection, GroupingViewModelOptions? options)
            where T : class, INotifyPropertyChanged
        {
            return new GroupingViewModel(commandBuilder, () => (ListCollectionView)CollectionViewSource.GetDefaultView(collection), typeof(T), options);
        }

        public static GroupingViewModel Create<T>(IScarletCommandBuilder commandBuilder, Func<ListCollectionView> collectionViewFactory)
            where T : class, INotifyPropertyChanged
        {
            return Create<T>(commandBuilder, collectionViewFactory, null);
        }

        public static GroupingViewModel Create<T>(IScarletCommandBuilder commandBuilder, Func<ListCollectionView> collectionViewFactory, GroupingViewModelOptions? options)
            where T : class, INotifyPropertyChanged
        {
            return new GroupingViewModel(commandBuilder, collectionViewFactory, typeof(T), options);
        }

        private readonly ConcurrentDictionary<string, GroupViewModel> _filterCollection;
        private readonly Func<ListCollectionView> _collectionViewFactory;
        private readonly Type _type;
        private readonly GroupingViewModelOptions? _options;

        private int _maxGroupings;
        private ListCollectionView? _view;

        private bool _isLiveGroupingEnabled;

        public bool IsLiveGroupingEnabled
        {
            get => _isLiveGroupingEnabled;
            set
            {
                if (!SetProperty(ref _isLiveGroupingEnabled, value))
                {
                    return;
                }

                _view?.IsLiveGrouping = value;
            }
        }

        private bool _isLiveSortingEnabled;

        public bool IsLiveSortingEnabled
        {
            get => _isLiveSortingEnabled;
            set
            {
                if (!SetProperty(ref _isLiveSortingEnabled, value))
                {
                    return;
                }

                if (_view is null)
                {
                    return;
                }

                using (_view.DeferRefresh())
                {
                    //var descriptions = _view.SortDescriptions.Cast<SortDescription>().DistinctBy(p => p.PropertyName).ToArray();
                    //_view.SortDescriptions.Clear();

                    _view.IsLiveSorting = value;

                    //_view.SortDescriptions.AddRange(descriptions);
                }
            }
        }

        public ICommand AddCommand { get; }

        public override ICommand RemoveCommand { get; }

        private GroupingViewModel(IScarletCommandBuilder commandBuilder, Func<ListCollectionView> collectionViewFactory, Type type, GroupingViewModelOptions? options)
            : base(commandBuilder)
        {
            _type = type ?? throw new ArgumentNullException(nameof(type));
            _options = options;
            _filterCollection = new ConcurrentDictionary<string, GroupViewModel>();
            _collectionViewFactory = collectionViewFactory ?? throw new ArgumentNullException(nameof(collectionViewFactory));

            AddCommand = commandBuilder.Create(Add, CanAdd)
                .WithBusyNotification(BusyStack)
                .WithSingleExecution()
                .WithCancellation()
                .Build();

            RemoveCommand = commandBuilder
                .Create<GroupsViewModel>(Remove, CanRemove)
                .WithSingleExecution()
                .WithBusyNotification(BusyStack)
                .WithCancellation()
                .Build();

            Messenger.Register<GroupingViewModel, ViewModelListBaseSelectionChanging<GroupViewModel>>(this, static (r, m) =>
            {
                if (m.Value is null || !ReferenceEquals(m.Value.Owner, r))
                {
                    return;
                }
                r._filterCollection.TryAdd(m.Value.Name, m.Value);
            });

            Messenger.Register<GroupingViewModel, ViewModelListBaseSelectionChanged<GroupViewModel>>(this, static (r, m) =>
            {
                if (m.Value is null || !ReferenceEquals(m.Value.Owner, r))
                {
                    return;
                }

                r._filterCollection.TryRemove(m.Value.Name, out var _);
            });
        }

        private ListCollectionView GetCollectionView()
        {
            _view ??= _collectionViewFactory.Invoke();

            using (_view.DeferRefresh())
            {
                _view.IsLiveGrouping = _isLiveGroupingEnabled;
                _view.IsLiveSorting = _isLiveSortingEnabled;
            }

            return _view;
        }

        public override async Task Remove(GroupsViewModel item, CancellationToken token)
        {
            await base.Remove(item, token).ConfigureAwait(false);
            Messenger.Send(new GroupsViewModelRemoved(item));

            var description = item.SelectedItem?.GroupDescription;
            if (description is not null)
            {
                await Dispatcher.Invoke(() => _view?.GroupDescriptions?.Remove(description)).ConfigureAwait(false);
            }

            if (item.SelectedItem is not null)
            {
                _filterCollection.TryAdd(item.SelectedItem.Name, item.SelectedItem);
            }

            // a removed instance must not keep reacting to the selections of the remaining ones
            item.Dispose();
        }

        /// <summary>
        /// Whether the collection is currently grouped by <paramref name="propertyName"/>
        /// </summary>
        public bool IsGroupedBy(string propertyName)
        {
            return FindGroups(propertyName) is not null;
        }

        /// <summary>
        /// Groups the collection by <paramref name="propertyName"/>, unless that grouping is already active. Loads this instance when necessary.
        /// </summary>
        /// <exception cref="ArgumentException"><paramref name="propertyName"/> is not available for grouping</exception>
        public async Task GroupBy(string propertyName, CancellationToken token)
        {
            ArgumentException.ThrowIfNullOrEmpty(propertyName);

            if (!IsLoaded)
            {
                await Load(token).ConfigureAwait(false);
            }

            if (IsGroupedBy(propertyName))
            {
                return;
            }

            if (!_filterCollection.TryGetValue(propertyName, out var group))
            {
                throw new ArgumentException($"{propertyName} is not available for grouping.", nameof(propertyName));
            }

            var result = new GroupsViewModel(CommandBuilder, GetCollectionView, this);
            await result.AddRange(_filterCollection.Values, token).ConfigureAwait(false);
            await Add(result, token).ConfigureAwait(false);

            await Dispatcher.Invoke(() => result.SelectedItem = group, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Removes the grouping by <paramref name="propertyName"/>, if there is one
        /// </summary>
        public async Task Ungroup(string propertyName, CancellationToken token)
        {
            ArgumentException.ThrowIfNullOrEmpty(propertyName);

            var groups = FindGroups(propertyName);
            if (groups is not null)
            {
                await Remove(groups, token).ConfigureAwait(false);
            }
        }

        private GroupsViewModel? FindGroups(string propertyName)
        {
            return Items.FirstOrDefault(p => p.SelectedItem?.Name == propertyName);
        }

        protected override Task RefreshInternal(CancellationToken token)
        {
            _filterCollection.Clear();

            return Task.Run(() =>
            {
                _type
                    .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .Where(p => p.CanRead && p.GetGetMethod(true)?.IsPublic == true)
                    .Where(p => _options?.IsIncluded(p) != false)
                    .Select(p => new GroupViewModel(CommandBuilder, p, this))
                    .Select(p => new KeyValuePair<string, GroupViewModel>(p.Name, p))
                    .ForEach(p => _filterCollection.TryAdd(p.Key, p.Value));

                _maxGroupings = _filterCollection.Count;
            }, token);
        }

        private async Task Add(CancellationToken token)
        {
            var result = new GroupsViewModel(CommandBuilder, GetCollectionView, this);
            await result.AddRange(_filterCollection.Values, token).ConfigureAwait(false);

            await Add(result, token).ConfigureAwait(false);
        }

        private bool CanAdd()
        {
            return !IsDisposed
                && !IsBusy
                && !_filterCollection.IsEmpty
                && _maxGroupings > Count;
        }

        public override async Task Clear(CancellationToken token)
        {
            foreach (var item in _items)
            {
                item.Dispose();
            }

            await base.Clear(token).ConfigureAwait(false);
        }
    }
}
