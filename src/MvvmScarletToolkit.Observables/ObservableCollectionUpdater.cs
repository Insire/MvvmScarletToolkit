using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace MvvmScarletToolkit.Observables
{
    // TODO write tests for this
    public abstract class ObservableCollectionUpdater<TViewModel, TKey>(Func<TViewModel, TKey> keyAccessor, IViewModelMerger<TViewModel> viewModelMerger, IEqualityComparer<TKey>? equalityComparer = null)
        where TViewModel : class, INotifyCollectionChanged
    {
        private readonly Func<TViewModel, TKey> _keyAccessor = keyAccessor;
        private readonly IViewModelMerger<TViewModel> _viewModelMerger = viewModelMerger;
        private readonly IEqualityComparer<TKey>? _equalityComparer = equalityComparer ?? EqualityComparer<TKey>.Default;

        public void Update(ObservableCollection<TViewModel> source, IEnumerable<TViewModel> changes)
        {
            var changesDictionary = changes.ToDictionary(p => _keyAccessor(p));
            if (changesDictionary.Count == 0)
            {
                // no changes ->  nothing to do
                return;
            }

            var sourceDictionary = source.ToDictionary(p => _keyAccessor(p));
            if (sourceDictionary.Count == 0)
            {
                // source empty, so add everything
                foreach (var insert in changesDictionary)
                {
                    source.Add(insert.Value);
                }

                return;
            }

            foreach (var delete in changesDictionary.Keys.Except(sourceDictionary.Keys, _equalityComparer))
            {
                var viewModel = changesDictionary[delete];
                source.Remove(viewModel);
            }

            foreach (var insert in sourceDictionary.Keys.Except(changesDictionary.Keys, _equalityComparer))
            {
                var viewModel = sourceDictionary[insert];
                source.Add(viewModel);
            }

            foreach (var update in sourceDictionary.Keys.Intersect(changesDictionary.Keys, _equalityComparer))
            {
                if (!sourceDictionary.TryGetValue(update, out var sourceInstance) || !changesDictionary.TryGetValue(update, out var changedInstance))
                {
                    continue;
                }

                var (resultingUpdate, replace) = _viewModelMerger.Perform(sourceInstance, changedInstance);

                if (!replace)
                {
                    continue;
                }

                // you really really dont want to replace an instance in general,
                // because scanning for the index gets more expensive the larger the collection is
                var index = source.IndexOf(sourceInstance);
                source[index] = resultingUpdate;
            }
        }
    }
}
