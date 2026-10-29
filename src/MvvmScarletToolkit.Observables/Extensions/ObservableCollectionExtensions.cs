using DynamicData.Binding;

namespace MvvmScarletToolkit
{
    public static class ObservableCollectionExtensions
    {
        /// <summary>
        /// in place sort an <see cref="IObservableCollection{T}"/>
        /// </summary>
        /// <remarks>
        /// will undo selection, when collection is bound for example to a ListBox
        /// </remarks>
        public static void Sort<T>(this IObservableCollection<T> collection)
            where T : IComparable
        {
            if (collection is null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            Sort(collection, collection.OrderBy(x => x).ToList());
        }

        /// <summary>
        /// in place sort an <see cref="IObservableCollection{T}"/> utilizing an <see cref="IComparer{T}"/>
        /// </summary>
        /// <remarks>
        /// will undo selection, when collection is bound for example to a ListBox
        /// </remarks>
        public static void Sort<T>(this IObservableCollection<T> collection, IComparer<T> comparer)
        {
            if (collection is null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            Sort(collection, collection.OrderBy(x => x, comparer).ToList());
        }

        private static void Sort<T>(IObservableCollection<T> collection, List<T> sorted)
        {
            var equalityComparer = EqualityComparer<T>.Default;

            for (var i = 0; i < sorted.Count; i++)
            {
                // everything before i is already in place, so only look at the unsorted remainder.
                // searching from the start would pick up already placed duplicates and undo previous moves
                var index = i;
                while (!equalityComparer.Equals(collection[index], sorted[i]))
                {
                    index++;
                }

                // avoid raising CollectionChanged for items that are already in place
                if (index != i)
                {
                    collection.Move(index, i);
                }
            }
        }
    }
}
