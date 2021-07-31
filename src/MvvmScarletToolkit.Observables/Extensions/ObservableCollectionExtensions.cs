using DynamicData.Binding;
using System;
using System.Linq;

namespace MvvmScarletToolkit
{
    public static class ObservableCollectionExtensions
    {
        /// <summary>
        /// in place sort an IObservableCollection<typeparamref name="T"/>
        /// </summary>
        /// <remarks>
        /// will undo selection, when collection is bound  for example to a ListBox
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
    }
}
