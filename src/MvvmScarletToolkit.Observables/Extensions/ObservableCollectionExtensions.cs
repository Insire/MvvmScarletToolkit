using DynamicData.Binding;
using System;
using System.Collections.Generic;
using System.Linq;

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
            var sorted = collection.OrderBy(x => x).ToList();

            for (var i = 0; i < sorted.Count; i++)
            {
                collection.Move(collection.IndexOf(sorted[i]), i);
            }
        }

        /// <summary>
        /// in place sort an <see cref="IObservableCollection{T}"/> utilizing an <see cref="IComparer{T}"/>
        /// </summary>
        /// <remarks>
        /// will undo selection, when collection is bound for example to a ListBox
        /// </remarks>
        public static void Sort<T>(this IObservableCollection<T> collection, IComparer<T> comparer)
        {
            var sorted = collection.OrderBy(x => x, comparer).ToList();

            for (var i = 0; i < sorted.Count; i++)
            {
                collection.Move(collection.IndexOf(sorted[i]), i);
            }
        }
    }
}
