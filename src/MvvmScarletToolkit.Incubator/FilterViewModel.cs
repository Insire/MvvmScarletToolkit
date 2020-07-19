using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Observables
{
    public sealed class FilterViewModel<T> : BusinessViewModelBase
    {
        private readonly IComparer<T> _comparer;
        private readonly IList<T> _source;
        private readonly ObservableCollection<T> _items;
        private readonly ReadOnlyCollection<Func<T, bool>> _predicates;

        private bool _forceEmptyView;

        public IReadOnlyCollection<T> Items { get; }

        public FilterViewModel(IScarletCommandBuilder commandBuilder, IList<T> source, Func<T, bool> predicate, IComparer<T> comparer)
            : this(commandBuilder, source, new[] { predicate }, comparer)
        {
        }

        public FilterViewModel(IScarletCommandBuilder commandBuilder, IList<T> source, Func<T, bool>[] predicates, IComparer<T> comparer)
            : base(commandBuilder)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
            _comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));

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

        protected override async Task LoadInternal(CancellationToken token)
        {
            using (BusyStack.GetToken())
            {
                _forceEmptyView = false;
                await Update().ConfigureAwait(false);
            }
        }

        public override bool CanLoad()
        {
            return !IsBusy && base.CanLoad();
        }

        protected override async Task UnloadInternal(CancellationToken token)
        {
            using (BusyStack.GetToken())
            {
                _forceEmptyView = true;
                await Update().ConfigureAwait(false);
            }
        }

        public override bool CanUnload()
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

        public override bool CanRefresh()
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
                var task = Dispatcher.Invoke(() => _items.Remove(item));
                tasks.Add(task);
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);
            tasks.Clear();

            foreach (var item in added)
            {
                var task = Dispatcher.Invoke(() => _items.Add(item));
                tasks.Add(task);
            }
            await Task.WhenAll(tasks).ConfigureAwait(false);

            await MergeSort_Recursive(_source, 0, _source.Count - 1).ConfigureAwait(false);
        }

        private async Task MergeSort_Recursive(IList<T> numbers, int left, int right)
        {
            int mid;

            if (right > left)
            {
                mid = (right + left) / 2;
                await MergeSort_Recursive(numbers, left, mid).ConfigureAwait(false);
                await MergeSort_Recursive(numbers, mid + 1, right).ConfigureAwait(false);

                await DoMerge(numbers, left, mid + 1, right).ConfigureAwait(false);
            }
        }

        private async Task DoMerge(IList<T> numbers, int left, int mid, int right)
        {
            var temp = new T[numbers.Count];
            int i, left_end, num_elements, tmp_pos;

            left_end = mid - 1;
            tmp_pos = left;
            num_elements = right - left + 1;

            while ((left <= left_end) && (mid <= right))
            {
                if (_comparer.Compare(numbers[left], numbers[mid]) <= 0)
                {
                    //if (numbers[left] <= numbers[mid])
                    temp[tmp_pos++] = numbers[left++];
                }
                else
                {
                    temp[tmp_pos++] = numbers[mid++];
                }
            }

            while (left <= left_end)
                temp[tmp_pos++] = numbers[left++];

            while (mid <= right)
                temp[tmp_pos++] = numbers[mid++];

            for (i = 0; i < num_elements; i++)
            {
                await Dispatcher.Invoke(() => numbers[right] = temp[right]).ConfigureAwait(false);
                right--;
            }
        }

        private IList<T> UpdateExisting()
        {
            var result = new List<T>(); // questionable, if i actually benefit from using a hashset here

            for (var i = _items.Count - 1; i >= 0; i--) // optional parallelization
            {
                for (var j = 0; j < _predicates.Count; j++)
                {
                    var item = _items[i];
                    var predicate = _predicates[j];

                    if (!predicate(item))
                    {
                        result.Add(item);
                    }
                }
            }
            result.Sort(_comparer);
            return result;
        }

        private IList<T> UpdateNew()
        {
            var result = new List<T>(); // questionable, if i actually benefit from using a hashset here
            var source = _source.ToArray();

            for (var i = source.Length - 1; i >= 0; i--) // optional parallelization
            {
                for (var j = 0; j < _predicates.Count - 1; j++)
                {
                    var item = source[i];
                    var predicate = _predicates[j];

                    if (predicate(item))
                    {
                        result.Add(item);
                    }
                }
            }

            result.Sort(_comparer);
            return result;
        }

        private bool ForceEmptyView()
        {
            return _forceEmptyView;
        }
    }
}
