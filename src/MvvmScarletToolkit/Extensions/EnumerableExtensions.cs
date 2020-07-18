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

        public static Task ForEachAsync<T>(this IEnumerable<T> source, in Func<T, Task> funcBody, in CancellationToken token)
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

        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> collection, in Action<T> action, in CancellationToken token)
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
    }
}
