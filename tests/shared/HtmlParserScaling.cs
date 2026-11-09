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
        private const int SmallRepetitions = 5_000;
        private const int LargeRepetitions = 4 * SmallRepetitions;
        private const long NoGCRegionSize = 128L * 1024 * 1024;

        /// <summary>
        /// Parses the same content at two sizes and compares the times. Linear parsing takes about 4 times as long for 4 times the input,
        /// quadratic parsing about 16 times. A ratio is independent of how fast the machine is, unlike a fixed time limit.
        /// </summary>
        public static void AssertLinear()
        {
            // warm up at full size, so the first measurement doesn't include jitting or the switch to optimized code
            Parse(SmallRepetitions);

            var (small, smallTags) = Measure(SmallRepetitions);
            var (large, largeTags) = Measure(LargeRepetitions);

            // below a millisecond the timer resolution would decide the ratio
            var ratio = large.TotalMilliseconds / Math.Max(small.TotalMilliseconds, 1);

            Assert.Multiple(() =>
            {
                // about 4 times, apart from the text after the last fragment
                Assert.InRange(largeTags, (4 * smallTags) - 4, 4 * smallTags);
                Assert.True(ratio < 8, $"parsing {SmallRepetitions} fragments took {small}, {LargeRepetitions} took {large}, a ratio of {ratio:0.0}");
            });
        }

        /// <summary>
        /// The fastest of three runs, so a busy build agent during one run doesn't decide the result.
        /// </summary>
        /// <remarks>
        /// Each run happens in a no GC region where possible. The whole tree stays alive while it is built, so a garbage collection
        /// during the large run walks a much bigger tree than one during the small run, which made the ratio about 8 to 14 for a linear parser.
        /// Runs whose region a parallel test ended with its allocations only count when no run kept its region.
        /// </remarks>
        private static (TimeSpan Elapsed, int Tags) Measure(int repetitions)
        {
            var best = TimeSpan.MaxValue;
            var bestWithGc = TimeSpan.MaxValue;
            var tags = 0;

            for (var i = 0; i < 3; i++)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                var inRegion = TryStartNoGCRegion();
                var watch = Stopwatch.StartNew();
                tags = Parse(repetitions);
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

            return (best == TimeSpan.MaxValue ? bestWithGc : best, tags);
        }

        private static TimeSpan Min(TimeSpan left, TimeSpan right)
        {
            return left < right ? left : right;
        }

        private static bool TryStartNoGCRegion()
        {
            try
            {
                // the large run allocates about 60 MB
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
