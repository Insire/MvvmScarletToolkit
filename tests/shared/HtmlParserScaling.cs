using MvvmScarletToolkit.Html;
using System.Diagnostics;

namespace MvvmScarletToolkit
{
    /// <summary>
    /// Checks that the shared html parser scales linearly. Linked into the WPF and Avalonia tests, which each compile their own copy of the parser.
    /// </summary>
    internal static class HtmlParserScaling
    {
        private const string Fragment = "text <b>bold</b> and <i>italic</i> ";
        private const int SmallRepetitions = 5_000;
        private const int LargeRepetitions = 4 * SmallRepetitions;

        /// <summary>
        /// Parses the same content at two sizes and compares the times. Linear parsing takes about 4 times as long for 4 times the input,
        /// quadratic parsing about 16 times. A ratio is independent of how fast the machine is, unlike a fixed time limit.
        /// </summary>
        public static void AssertLinear()
        {
            // warm up, so the first measurement doesn't include jitting
            Parse(100);

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
        /// The fastest of three runs, so a garbage collection or a busy build agent during one run doesn't decide the result.
        /// </summary>
        private static (TimeSpan Elapsed, int Tags) Measure(int repetitions)
        {
            var best = TimeSpan.MaxValue;
            var tags = 0;

            for (var i = 0; i < 3; i++)
            {
                var watch = Stopwatch.StartNew();
                tags = Parse(repetitions);
                watch.Stop();

                if (watch.Elapsed < best)
                {
                    best = watch.Elapsed;
                }
            }

            return (best, tags);
        }

        private static int Parse(int repetitions)
        {
            var tree = new HtmlTagTree();
            tree.BuildFrom(string.Concat(Enumerable.Repeat(Fragment, repetitions)));

            return tree.GetTags().Count();
        }
    }
}
