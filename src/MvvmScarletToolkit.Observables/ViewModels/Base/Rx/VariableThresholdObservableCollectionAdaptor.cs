using DynamicData;
using DynamicData.Binding;
using System;
using System.Linq;
using System.Reactive.Linq;

namespace MvvmScarletToolkit
{
    // https://github.com/reactivemarbles/DynamicData/blob/36d18e0f34457d4682fea1d68e2e65e8800f87fc/src/DynamicData/Binding/ObservableCollectionAdaptor.cs
    public class VariableThresholdObservableCollectionAdaptor<TObject, TKey> : ISortedObservableCollectionAdaptor<TObject, TKey>
        where TKey : notnull
    {
        private readonly Func<int> _refreshThreshold;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object" /> class.
        /// </summary>
        /// <param name="refreshThreshold">The number of changes before a Reset event is used</param>
        public VariableThresholdObservableCollectionAdaptor(Func<int> refreshThreshold)
        {
            _refreshThreshold = refreshThreshold;
        }

        /// <summary>
        /// Maintains the specified collection from the changes
        /// </summary>
        /// <param name="changes">The changes.</param>
        /// <param name="collection">The collection.</param>
        public void Adapt(ISortedChangeSet<TObject, TKey> changes, IObservableCollection<TObject> collection)
        {
            switch (changes.SortedItems.SortReason)
            {
                case SortReason.InitialLoad:
                    {
                        if (changes.Count > _refreshThreshold())
                        {
                            using (collection.SuspendNotifications())
                            {
                                collection.Load(changes.SortedItems.Select(kv => kv.Value));
                            }
                        }
                        else
                        {
                            using (collection.SuspendCount())
                            {
                                DoUpdate(changes, collection);
                            }
                        }
                    }
                    break;

                case SortReason.ComparerChanged:
                case SortReason.Reset:
                    using (collection.SuspendNotifications())
                    {
                        collection.Load(changes.SortedItems.Select(kv => kv.Value));
                    }
                    break;

                case SortReason.DataChanged:
                    if (changes.Count > _refreshThreshold())
                    {
                        using (collection.SuspendNotifications())
                        {
                            collection.Load(changes.SortedItems.Select(kv => kv.Value));
                        }
                    }
                    else
                    {
                        using (collection.SuspendCount())
                        {
                            DoUpdate(changes, collection);
                        }
                    }
                    break;

                case SortReason.Reorder:
                    //Updates will only be moves, so apply logic
                    using (collection.SuspendCount())
                    {
                        DoUpdate(changes, collection);
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void DoUpdate(ISortedChangeSet<TObject, TKey> updates, IObservableCollection<TObject> list)
        {
            foreach (var update in updates)
            {
                switch (update.Reason)
                {
                    case ChangeReason.Add:
                        list.Insert(update.CurrentIndex, update.Current);
                        break;

                    case ChangeReason.Remove:
                        list.RemoveAt(update.CurrentIndex);
                        break;

                    case ChangeReason.Moved:
                        list.Move(update.PreviousIndex, update.CurrentIndex);
                        break;

                    case ChangeReason.Update:
                        list.RemoveAt(update.PreviousIndex);
                        list.Insert(update.CurrentIndex, update.Current);
                        break;
                }
            }
        }
    }
}
