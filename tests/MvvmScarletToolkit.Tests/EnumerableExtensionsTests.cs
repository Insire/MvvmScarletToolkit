using System.Collections.Concurrent;

namespace MvvmScarletToolkit.Tests
{
    public sealed class EnumerableExtensionsTests : TraceTestBase
    {
        [Fact]
        public async Task ForEachAsync_Should_Visit_Every_Item()
        {
            var visited = new ConcurrentBag<int>();

            await Enumerable.Range(0, 100).ForEachAsync(p =>
            {
                visited.Add(p);
                return Task.CompletedTask;
            });

            Assert.Equal(Enumerable.Range(0, 100), visited.Order());
        }

        [Fact]
        public async Task ForEachAsync_With_Token_Should_Visit_Every_Item()
        {
            var visited = new ConcurrentBag<int>();

            await Enumerable.Range(0, 100).ForEachAsync(p =>
            {
                visited.Add(p);
                return Task.CompletedTask;
            }, TestContext.Current.CancellationToken);

            Assert.Equal(100, visited.Count);
        }

        [Fact]
        public async Task ForEachAsync_Should_Not_Exceed_The_Degree_Of_Parallelism()
        {
            const int maxDoP = 3;
            var running = 0;
            var peak = 0;

            await Enumerable.Range(0, 30).ForEachAsync(async _ =>
            {
                var current = Interlocked.Increment(ref running);
                InterlockedMax(ref peak, current);

                await Task.Delay(1, TestContext.Current.CancellationToken);

                Interlocked.Decrement(ref running);
            }, maxDoP, TestContext.Current.CancellationToken);

            Assert.InRange(peak, 1, maxDoP);
        }

        private static void InterlockedMax(ref int target, int value)
        {
            int current;
            do
            {
                current = Volatile.Read(ref target);
            }
            while (value > current && Interlocked.CompareExchange(ref target, value, current) != current);
        }

        [Fact]
        public async Task ForEachAsync_Should_Stop_Starting_Items_After_Cancellation()
        {
            using var cancellation = new CancellationTokenSource();
            var visited = 0;

            await Enumerable.Range(0, 1000).ForEachAsync(_ =>
            {
                Interlocked.Increment(ref visited);
                cancellation.Cancel();
                return Task.CompletedTask;
            }, 1, cancellation.Token);

            Assert.Equal(1, visited);
        }

        [Fact]
        public async Task ForEachAsync_Should_Propagate_Exceptions()
        {
            var source = new[] { 1 };

            await Assert.ThrowsAsync<InvalidOperationException>(() => source.ForEachAsync(_ => throw new InvalidOperationException()));
        }

        [Fact]
        public void ForEach_Should_Visit_Items_In_Order_And_Return_The_Source()
        {
            IEnumerable<int> source = [1, 2, 3];
            var visited = new List<int>();

            var result = source.ForEach(visited.Add);

            Assert.Multiple(() =>
            {
                Assert.Equal([1, 2, 3], visited);
                Assert.Same(source, result);
            });
        }

        [Fact]
        public void ForEach_Should_Stop_After_Cancellation()
        {
            using var cancellation = new CancellationTokenSource();
            IEnumerable<int> source = [1, 2, 3];
            var visited = new List<int>();

            var result = source.ForEach(p =>
            {
                visited.Add(p);
                cancellation.Cancel();
            }, cancellation.Token);

            Assert.Multiple(() =>
            {
                Assert.Equal([1], visited);
                Assert.Same(source, result);
            });
        }

        [Fact]
        public void Dispose_Should_Dispose_Every_Item()
        {
            var items = new[] { Substitute.For<IDisposable>(), Substitute.For<IDisposable>() };

            ((IEnumerable<IDisposable>)items).Dispose();

            Assert.Multiple(() =>
            {
                items[0].Received(1).Dispose();
                items[1].Received(1).Dispose();
            });
        }

        [Fact]
        public void DistinctBy_Should_Keep_The_First_Item_Per_Key()
        {
            // the qualified call, System.Linq has the same extension on newer frameworks
            var result = EnumerableExtensions.DistinctBy(["apple", "avocado", "banana", "blueberry", "cherry"], p => p[0]);

            Assert.Equal(["apple", "banana", "cherry"], result);
        }

        [Fact]
        public void AddRange_Should_Add_All_Items()
        {
            ICollection<int> target = new HashSet<int> { 1 };

            target.AddRange([2, 3]);

            Assert.Equal([1, 2, 3], target.Order());
        }

        [Fact]
        public void Batch_Should_Split_Into_Full_Batches()
        {
            var result = Enumerable.Range(1, 4).Batch(2).Select(p => p.ToArray()).ToArray();

            Assert.Equal([[1, 2], [3, 4]], result);
        }

        [Fact]
        public void Batch_Should_Return_The_Remainder_As_A_Smaller_Batch()
        {
            var result = Enumerable.Range(1, 5).Batch(2).Select(p => p.ToArray()).ToArray();

            Assert.Equal([[1, 2], [3, 4], [5]], result);
        }

        [Fact]
        public void Batch_Should_Return_Nothing_For_An_Empty_Source()
        {
            Assert.Empty(Enumerable.Empty<int>().Batch(2));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Batch_Should_Return_Nothing_For_An_Invalid_Size(int size)
        {
            Assert.Empty(Enumerable.Range(1, 5).Batch(size));
        }
    }
}
