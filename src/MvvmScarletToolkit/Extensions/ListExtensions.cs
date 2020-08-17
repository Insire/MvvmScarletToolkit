using System;
using System.Collections.Generic;
using System.Linq;

namespace MvvmScarletToolkit
{
    // source: https://github.com/duanenewman/SantasBloggers/blob/master/IListExtensions.cs
    // var updatedBloggers = BloggerService.GetSantasBloggers();
    // var modelToViewModelMatcher = new Func<BloggerViewModel, Blogger, bool>((vm, model) => vm.Id == model.Id);
    // var viewModelUpdater = new Action<BloggerViewModel, Blogger>((vm, model) => { vm.NaughtyNiceRating = model.NaughtyNiceRating; vm.JustAdded = false; });
    // Dispatcher.BeginInvoke(() =>
    // {
    //  SantasBloggers.UpdateItems(updatedBloggers, modelToViewModelMatcher, MapBloggerToBloggerViewModel, viewModelUpdater);
    // });
    public static class ListExtensions
    {
        public static void UpdateItems<T1, T2>(this IList<T1> targetCollection, in IEnumerable<T2> updateCollection, in Func<T1, T2, bool> comparer, in Func<T2, T1> mapper)
        {
            UpdateItems(targetCollection, updateCollection, comparer, mapper, null, _ => true, _ => false);
        }

        public static void UpdateItems<T1, T2>(this IList<T1> targetCollection, in IEnumerable<T2> updateCollection, in Func<T1, T2, bool> comparer, in Func<T2, T1> mapper, in Action<T1, T2>? updater)
        {
            UpdateItems(targetCollection, updateCollection, comparer, mapper, updater, _ => true, _ => false);
        }

        public static void UpdateItems<T1, T2>(this IList<T1> targetCollection, in IEnumerable<T2> updateCollection, in Func<T1, T2, bool> comparer, in Func<T2, T1> mapper, in Action<T1, T2>? updater, in Func<T1, bool> targetFilter)
        {
            UpdateItems(targetCollection, updateCollection, comparer, mapper, updater, targetFilter, _ => false);
        }

        public static void UpdateItems<T1, T2>(this IList<T1> targetCollection, in IEnumerable<T2> updateCollection, in Func<T1, T2, bool> comparer, in Func<T2, T1> mapper, in Action<T1, T2>? updater, in Func<T1, bool> targetFilter, in Func<T1, bool> ignoreItem)
        {
            if (updateCollection is null)
            {
                return;
            }

            var itemsToRemove = new List<T1>();

            UpdateExistingItemsAndIdentifyRemovedItems(targetCollection, updateCollection, comparer, updater, itemsToRemove, targetFilter, ignoreItem);

            RemoveOldItemsFromTarget(targetCollection, itemsToRemove);

            AddNewItemsToTarget(targetCollection, updateCollection, comparer, mapper);
        }

        private static void UpdateExistingItemsAndIdentifyRemovedItems<T1, T2>(
            in IList<T1> targetCollection,
            in IEnumerable<T2> updateCollection,
            Func<T1, T2, bool> comparer,
            in Action<T1, T2>? updater,
            List<T1> itemsToRemove,
            in Func<T1, bool> targetFilter,
            in Func<T1, bool> ignoreItem)
        {
            var array = targetCollection.Where(targetFilter).ToArray();
            for (var i = 0; i < array.Length; i++)
            {
                var targetItem = array[i];
                var updateItem = updateCollection.FirstOrDefault(u => comparer(targetItem, u));
                if (updateItem is null && !ignoreItem(targetItem))
                {
                    itemsToRemove.Add(targetItem);
                    continue;
                }

                updater?.Invoke(targetItem, updateItem);
            }
        }

        private static void RemoveOldItemsFromTarget<T1>(ICollection<T1> targetCollection, in IEnumerable<T1> itemsToRemove)
        {
            foreach (var item in itemsToRemove)
            {
                targetCollection.Remove(item);
            }
        }

        private static void AddNewItemsToTarget<T1, T2>(IList<T1> targetCollection, in IEnumerable<T2> updateCollection, Func<T1, T2, bool> comparer, in Func<T2, T1> mapper)
        {
            var itemsToAdd = updateCollection.Where(updateItem
                => !targetCollection.Any(existingITem => comparer(existingITem, updateItem))).ToList();

            foreach (var item in itemsToAdd)
            {
                targetCollection.Add(mapper(item));
            }
        }
    }
}
