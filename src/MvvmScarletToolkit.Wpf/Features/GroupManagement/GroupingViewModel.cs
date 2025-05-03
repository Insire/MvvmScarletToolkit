using CommunityToolkit.Mvvm.Messaging;
using MvvmScarletToolkit.Observables;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
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
            return new GroupingViewModel(commandBuilder, () => (ListCollectionView)CollectionViewSource.GetDefaultView(collection), typeof(T));
        }

        public static GroupingViewModel Create<T>(IScarletCommandBuilder commandBuilder, Func<ListCollectionView> collectionViewFactory)
            where T : class, INotifyPropertyChanged
        {
            return new GroupingViewModel(commandBuilder, collectionViewFactory, typeof(T));
        }

        private readonly ConcurrentDictionary<string, GroupViewModel> _filterCollection;
        private readonly Func<ListCollectionView> _collectionViewFactory;
        private readonly Type _type;

        private int _maxGroupings;
        private ListCollectionView? _view;

        private bool _isLiveGroupingEnabled;

        public bool IsLiveGroupingEnabled
        {
            get => _isLiveGroupingEnabled;
            set
            {
                if (SetProperty(ref _isLiveGroupingEnabled, value))
                {
                    if (_view is null)
                    {
                        return;
                    }

                    _view.IsLiveGrouping = value;
                }
            }
        }

        private bool _isLiveSortingEnabled;

        public bool IsLiveSortingEnabled
        {
            get => _isLiveSortingEnabled;
            set
            {
                if (SetProperty(ref _isLiveSortingEnabled, value))
                {
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
        }

        public ICommand AddCommand { get; }

        public override ICommand RemoveCommand { get; }

        private GroupingViewModel(IScarletCommandBuilder commandBuilder, Func<ListCollectionView> collectionViewFactory, Type type)
            : base(commandBuilder)
        {
            _type = type ?? throw new ArgumentNullException(nameof(type));
            _filterCollection = new ConcurrentDictionary<string, GroupViewModel>();
            _collectionViewFactory = collectionViewFactory ?? throw new ArgumentNullException(nameof(collectionViewFactory));

            AddCommand = commandBuilder.Create(Add, CanAdd)
                .WithBusyNotification(BusyStack)
                .WithSingleExecution()
                .WithCancellation()
                .Build();

            RemoveCommand = commandBuilder
                .Create<GroupsViewModel>((p, t) => Remove(p, t), (p) => CanRemove(p))
                .WithSingleExecution()
                .WithBusyNotification(BusyStack)
                .WithCancellation()
                .Build();

            Messenger.Register<GroupingViewModel, ViewModelListBaseSelectionChanging<GroupViewModel>>(this, (r, m) =>
            {
                if (m.Value is null)
                {
                    return;
                }

                r._filterCollection.TryAdd(m.Value.Name, m.Value);
            });

            Messenger.Register<GroupingViewModel, ViewModelListBaseSelectionChanged<GroupViewModel>>(this, (r, m) =>
            {
                if (m.Value is null)
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
                await Dispatcher.Invoke(() => _view?.GroupDescriptions.Remove(description)).ConfigureAwait(false);
            }

            if (item.SelectedItem is not null)
            {
                _filterCollection.TryAdd(item.SelectedItem.Name, item.SelectedItem);
            }
        }

        protected override Task RefreshInternal(CancellationToken token)
        {
            _filterCollection.Clear();

            return Task.Run(() =>
            {
                _type
                    .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .Where(p => p.CanRead && p.GetGetMethod(true)?.IsPublic == true)
                    .Select(p => new GroupViewModel(CommandBuilder, p))
                    .Select(p => new KeyValuePair<string, GroupViewModel>(p.Name, p))
                    .ForEach(p => _filterCollection.TryAdd(p.Key, p.Value));

                _maxGroupings = _filterCollection.Count;
            }, token);
        }

        private async Task Add(CancellationToken token)
        {
            var result = new GroupsViewModel(CommandBuilder, GetCollectionView);
            await result.AddRange(_filterCollection.Values, token).ConfigureAwait(false);

            await Add(result, token).ConfigureAwait(false);
        }

        private bool CanAdd()
        {
            return !IsDisposed
                && !IsBusy
                && _filterCollection?.Count > 0
                && _maxGroupings > Count;
        }

        public override async Task Clear(CancellationToken token)
        {
            for (var i = 0; i < _items.Count; i++)
            {
                var item = _items[i];
                item.Dispose();
            }

            await base.Clear(token).ConfigureAwait(false);
        }
    }
}
