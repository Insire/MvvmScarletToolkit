using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit
{
    public static class EnumerableExtensions
    {
        private const int Multiplier = 2;

        public static Task ForEachAsync<T>(this IEnumerable<T> source, in Func<T, Task> funcBody)
        {
            return source.ForEachAsync(funcBody, Environment.ProcessorCount * Multiplier, CancellationToken.None);
        }

        public static Task ForEachAsync<T>(this IEnumerable<T> source, in Func<T, Task> funcBody, CancellationToken token)
        {
            return source.ForEachAsync(funcBody, Environment.ProcessorCount * Multiplier, token);
        }

        public static Task ForEachAsync<T>(this IEnumerable<T> source, Func<T, Task> funcBody, int maxDoP, CancellationToken token)
        {
            async Task AwaitPartition(IEnumerator<T> partition)
            {
                while (partition.MoveNext() && !token.IsCancellationRequested)
                {
                    await funcBody(partition.Current).ConfigureAwait(false);
                }
            }

            return Task.WhenAll(
                Partitioner
                    .Create(source)
                    .GetPartitions(maxDoP)
                    .Select(AwaitPartition));
        }

        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> collection, in Action<T> action)
        {
            return collection.ForEach(action, CancellationToken.None);
        }

        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> collection, in Action<T> action, CancellationToken token)
        {
            foreach (var item in collection)
            {
                if (token.IsCancellationRequested)
                {
                    return collection;
                }

                action(item);
            }

            return collection;
        }

        public static void Dispose(this IEnumerable<IDisposable> source)
        {
            source.ForEach(p => p.Dispose());
        }

        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            var seenKeys = new HashSet<TKey>();
            foreach (var element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }

        public static void AddRange<T>(this ICollection<T> source, IEnumerable<T> items)
        {
            items.ForEach(p => source.Add(p));
        }

        // source: https://stackoverflow.com/a/13731823
        public static IEnumerable<IEnumerable<TSource>> Batch<TSource>(this IEnumerable<TSource> source, int size)
        {
            if (size <= 0)
            {
                yield break;
            }

            TSource[]? bucket = null;
            var count = 0;

            foreach (var item in source)
            {
                bucket ??= new TSource[size];

                bucket[count++] = item;
                if (count != size)
                {
                    continue;
                }

                yield return bucket;

                bucket = null;
                count = 0;
            }

            if (bucket != null && count > 0)
            {
                yield return bucket.Take(count).ToArray();
            }
        }
    }
}