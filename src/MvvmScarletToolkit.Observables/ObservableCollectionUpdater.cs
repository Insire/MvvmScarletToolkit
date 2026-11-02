using System.Collections.ObjectModel;

namespace MvvmScarletToolkit.Observables
{
    /// <summary>
    /// Synchronizes an <see cref="ObservableCollection{T}"/> with a new set of instances
    /// </summary>
    /// <remarks>
    /// instances missing in the changes are removed, new instances are appended and instances present in both are merged via <see cref="IViewModelMerger{TViewModel}"/>.
    /// An empty set of changes is treated as "nothing to do" and does not clear the collection.
    /// </remarks>
    public abstract class ObservableCollectionUpdater<TViewModel, TKey>(Func<TViewModel, TKey> keyAccessor, IViewModelMerger<TViewModel> viewModelMerger, IEqualityComparer<TKey>? equalityComparer = null)
        where TViewModel : class
        where TKey : notnull
    {
        private readonly Func<TViewModel, TKey> _keyAccessor = keyAccessor ?? throw new ArgumentNullException(nameof(keyAccessor));
        private readonly IViewModelMerger<TViewModel> _viewModelMerger = viewModelMerger ?? throw new ArgumentNullException(nameof(viewModelMerger));
        private readonly IEqualityComparer<TKey> _equalityComparer = equalityComparer ?? EqualityComparer<TKey>.Default;

        public void Update(ObservableCollection<TViewModel> source, IEnumerable<TViewModel> changes)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (changes is null)
            {
                throw new ArgumentNullException(nameof(changes));
            }

            var changesDictionary = changes.ToDictionary(_keyAccessor, _equalityComparer);
            if (changesDictionary.Count == 0)
            {
                // no changes ->  nothing to do
                return;
            }

            if (source.Count == 0)
            {
                // source empty, so add everything
                foreach (var insert in changesDictionary.Values)
                {
                    source.Add(insert);
                }

                return;
            }

            var sourceKeys = new HashSet<TKey>(_equalityComparer);

            // iterate backwards, so that removing entries doesn't shift the indices still to be visited
            // and so that replacing an instance doesn't require scanning for its index
            for (var i = source.Count - 1; i >= 0; i--)
            {
                var sourceInstance = source[i];
                var key = _keyAccessor(sourceInstance);

                if (!changesDictionary.TryGetValue(key, out var changedInstance))
                {
                    source.RemoveAt(i);
                    continue;
                }

                sourceKeys.Add(key);

                var (resultingUpdate, replace) = _viewModelMerger.Perform(sourceInstance, changedInstance);
                if (replace)
                {
                    source[i] = resultingUpdate;
                }
            }

            foreach (var insert in changesDictionary)
            {
                if (!sourceKeys.Contains(insert.Key))
                {
                    source.Add(insert.Value);
                }
            }
        }
    }
}
