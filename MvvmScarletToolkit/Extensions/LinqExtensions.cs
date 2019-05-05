using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MvvmScarletToolkit
{
    public static class LinqExtensions
    {
        public static Task ForEachAsync<T>(this IEnumerable<T> source, Func<T, Task> funcBody)
        {
            return source.ForEachAsync(funcBody, 4);
        }

        public static Task ForEachAsync<T>(this IEnumerable<T> source, Func<T, Task> funcBody, int maxDoP)
        {
            async Task AwaitPartition(IEnumerator<T> partition)
            {
                using (partition)
                {
                    while (partition.MoveNext())
                    {
                        await funcBody(partition.Current).ConfigureAwait(false);
                    }
                }
            }

            return Task.WhenAll(
                Partitioner
                    .Create(source)
                    .GetPartitions(maxDoP)
                    .Select(AwaitPartition));
        }
    }
}
