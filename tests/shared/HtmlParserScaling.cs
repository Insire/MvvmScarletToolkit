using MvvmScarletToolkit.Html;
using System.Diagnostics;
using System.Runtime;

namespace MvvmScarletToolkit
{
    /// <summary>
    /// Runs the timing test on its own, after the parallel tests, so their allocations and CPU load don't distort the measurement.
    /// </summary>
    [CollectionDefinition(nameof(HtmlParserScalingCollection), DisableParallelization = true)]
    public sealed class HtmlParserScalingCollection;

    [Collection(nameof(HtmlParserScalingCollection))]
    public sealed class HtmlParserScalingTests : TraceTestBase
    {
        [Fact]
        public void Parser_Should_Scale_Linearly()
        {
            // the previous parser copied the rest of the input for every tag, 20000 tags took about 6 seconds.
            // Compares two sizes instead of a fixed limit, which fails on slow build agents or with coverage enabled
            HtmlParserScaling.AssertLinear();
        }
    }

    /// <summary>
    /// Checks that the shared html parser scales linearly. Linked into the WPF and Avalonia tests, which each compile their own copy of the parser.
    /// </summary>
    internal static class HtmlParserScaling
    {
        private const string Fragment = "text <b>bold</b> and <i>italic</i> ";
        private const int Factor = 8;
        private const int SmallRepetitions = 5_000;
        private const int LargeRepetitions = Factor * SmallRepetitions;
        private const long NoGCRegionSize = 240L * 1024 * 1024;

        /// <summary>
        /// Linear parsing allocates about <see cref="Factor"/> times as much for <see cref="Factor"/> times the input, quadratic parsing,
        /// like the previous parser that copied the rest of the input for every tag, about <see cref="Factor"/> squared.
        /// </summary>
        private const double MaxAllocationRatio = 2 * Factor;

        /// <summary>
        /// Linear about 8, quadratic about 64. Far above the linear ratio, because other test processes run at the same time
        /// on the build agent and slow down the large run more than the small one, an 8 for 4 times the input was measured there.
        /// </summary>
        private const double MaxTimeRatio = 4 * Factor;

        /// <summary>
        /// Parses the same content at two sizes and compares allocations and times. Ratios are independent of how fast the machine is,
        /// unlike a fixed limit. Allocations are also independent of how busy the machine is, so they catch a quadratic parser reliably,
        /// the times catch quadratic work without allocations.
        /// </summary>
        public static void AssertLinear()
        {
            // warm up at full size, so the first measurement doesn't include jitting or the switch to optimized code
            Parse(SmallRepetitions);

            var (smallBytes, smallTags) = MeasureAllocations(SmallRepetitions);
            var (largeBytes, largeTags) = MeasureAllocations(LargeRepetitions);
            var small = MeasureTime(SmallRepetitions);
            var large = MeasureTime(LargeRepetitions);

            var allocationRatio = (double)largeBytes / Math.Max(smallBytes, 1);

            // below a millisecond the timer resolution would decide the ratio
            var timeRatio = large.TotalMilliseconds / Math.Max(small.TotalMilliseconds, 1);

            Assert.Multiple(() =>
            {
                // Factor times, apart from the text after the last fragment
                Assert.InRange(largeTags, (Factor * smallTags) - Factor, Factor * smallTags);
                Assert.True(allocationRatio < MaxAllocationRatio, $"parsing {SmallRepetitions} fragments allocated {smallBytes} bytes, {LargeRepetitions} allocated {largeBytes}, a ratio of {allocationRatio:0.0}");
                Assert.True(timeRatio < MaxTimeRatio, $"parsing {SmallRepetitions} fragments took {small}, {LargeRepetitions} took {large}, a ratio of {timeRatio:0.0}");
            });
        }

        /// <summary>
        /// Counts the bytes allocated by this thread, which other tests and processes don't influence.
        /// </summary>
        private static (long Bytes, int Tags) MeasureAllocations(int repetitions)
        {
            var before = GC.GetAllocatedBytesForCurrentThread();
            var tags = Parse(repetitions);

            return (GC.GetAllocatedBytesForCurrentThread() - before, tags);
        }

        /// <summary>
        /// The fastest of three runs, so a busy build agent during one run doesn't decide the result.
        /// </summary>
        /// <remarks>
        /// Each run happens in a no GC region where possible. The whole tree stays alive while it is built, so a garbage collection
        /// during the large run walks a much bigger tree than one during the small run, which made the ratio about 8 to 14 for a linear parser.
        /// Runs whose region a parallel test ended with its allocations only count when no run kept its region.
        /// </remarks>
        private static TimeSpan MeasureTime(int repetitions)
        {
            var best = TimeSpan.MaxValue;
            var bestWithGc = TimeSpan.MaxValue;

            for (var i = 0; i < 3; i++)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                var inRegion = TryStartNoGCRegion();
                var watch = Stopwatch.StartNew();
                Parse(repetitions);
                watch.Stop();

                if (inRegion && TryEndNoGCRegion())
                {
                    best = Min(best, watch.Elapsed);
                }
                else
                {
                    bestWithGc = Min(bestWithGc, watch.Elapsed);
                }
            }

            return best == TimeSpan.MaxValue ? bestWithGc : best;
        }

        private static TimeSpan Min(TimeSpan left, TimeSpan right)
        {
            return left < right ? left : right;
        }

        private static bool TryStartNoGCRegion()
        {
            try
            {
                // the large run allocates about 130 MB
                return GC.TryStartNoGCRegion(NoGCRegionSize);
            }
            catch (Exception ex) when (ex is InvalidOperationException or ArgumentOutOfRangeException)
            {
                return false;
            }
        }

        /// <summary>
        /// Ends the no GC region, <see langword="false"/> when a garbage collection already ended it.
        /// </summary>
        private static bool TryEndNoGCRegion()
        {
            if (GCSettings.LatencyMode != GCLatencyMode.NoGCRegion)
            {
                return false;
            }

            try
            {
                GC.EndNoGCRegion();
                return true;
            }
            catch (InvalidOperationException)
            {
                return false;
            }
        }

        private static int Parse(int repetitions)
        {
            var tree = new HtmlTagTree();
            tree.BuildFrom(string.Concat(Enumerable.Repeat(Fragment, repetitions)));

            return tree.GetTags().Count();
        }
    }
}
