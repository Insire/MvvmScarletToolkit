using MvvmScarletToolkit.Abstractions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Observables
{
    public sealed class FilterViewModel<T> : ViewModelBase
    {
        private readonly IComparer<T> _comparer;
        private readonly IEqualityComparer<T> _equalityComparer;
        private readonly IScarletDispatcher _dispatcher;
        private readonly IEnumerable<T> _source;
        private readonly ObservableCollection<T> _items;
        private readonly ReadOnlyCollection<Func<T, bool>> _predicates;

        private bool _forceEmptyView;

        public IReadOnlyCollection<T> Items { get; }

        public FilterViewModel(IScarletDispatcher dispatcher, IEnumerable<T> source, IReadOnlyCollection<Func<T, bool>> predicates, IEqualityComparer<T> equalityComparer, IComparer<T> comparer)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
            _comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
            _equalityComparer = equalityComparer ?? EqualityComparer<T>.Default;
            _items = new ObservableCollection<T>(source); // copies every item from the enumeration into the collection (no projection)
            _predicates = new ReadOnlyCollection<Func<T, bool>>(
                new[]
                {
                    new Func<T,bool>( _ => ForceEmptyView()),
                }
                .Concat(predicates)
                .ToArray());

            Items = new ReadOnlyObservableCollection<T>(_items);
        }

        public FilterViewModel(IScarletDispatcher dispatcher, IEnumerable<T> source, IReadOnlyCollection<Func<T, bool>> predicates)
            : this(dispatcher, source, predicates, null, null) // TODO
        {
        }

        protected override async Task LoadInternal(CancellationToken token)
        {
            using (BusyStack.GetToken())
            {
                _forceEmptyView = false;
                await Update().ConfigureAwait(false);
            }
        }

        protected override bool CanLoad()
        {
            return !IsBusy && base.CanLoad();
        }

        protected override async Task UnloadInternalAsync()
        {
            using (BusyStack.GetToken())
            {
                _forceEmptyView = true;
                await Update().ConfigureAwait(false);
            }
        }

        protected override bool CanUnload()
        {
            return !IsBusy && base.CanUnload();
        }

        protected override async Task RefreshInternal(CancellationToken token)
        {
            using (BusyStack.GetToken())
            {
                await Update().ConfigureAwait(false);
            }
        }

        protected override bool CanRefresh()
        {
            return !IsBusy && base.CanRefresh();
        }

        private async Task Update()
        {
            var addedTask = Task.Run(() => UpdateNew());
            var removedTask = Task.Run(() => UpdateExisting());

            await Task.WhenAll(addedTask, removedTask).ConfigureAwait(false);

            var added = addedTask.Result;
            var removed = removedTask.Result;

            var tasks = new List<Task>();

            foreach (var item in removed)
            {
                var task = _dispatcher.Invoke(() => _items.Remove(item));
                tasks.Add(task);
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);
            tasks.Clear();

            foreach (var item in added)
            {
                for (var i = 0; i < _items.Count - 1; i++)
                {
                    switch (_comparer.Compare(item, _items[i]))
                    {
                        case 1:
                            break;

                        case 0:
                            break;

                        case -1:
                            break;
                    }

                    //_items.Insert
                }
                //await _dispatcher.Invoke(() => _items.Add(item));
            }
        }

        private HashSet<T> UpdateExisting()
        {
            var result = new HashSet<T>(_equalityComparer); // questionable, if i actually benefit from using a hashset here

            for (var i = _items.Count; i >= 0; i--) // optional parallelization
            {
                for (var j = 0; j < _predicates.Count; j++)
                {
                    var item = _items[i];
                    var predicate = _predicates[i];

                    if (!predicate(item))
                    {
                        result.Add(item);
                    }
                }
            }

            return result;
        }

        private HashSet<T> UpdateNew()
        {
            var result = new HashSet<T>(_equalityComparer); // questionable, if i actually benefit from using a hashset here
            var source = _source.ToArray();

            for (var i = source.Length; i >= 0; i--) // optional parallelization
            {
                for (var j = 0; j < _predicates.Count; j++)
                {
                    var item = source[i];
                    var predicate = _predicates[i];

                    if (predicate(item))
                    {
                        result.Add(item);
                    }
                }
            }

            return result;
        }

        private bool ForceEmptyView()
        {
            return _forceEmptyView;
        }
    }
}
