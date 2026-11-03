using MvvmScarletToolkit.Wpf.Tests.Util;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Documents;

namespace MvvmScarletToolkit.Wpf.Tests
{
    public sealed class HtmlTextBlockTests : TraceTestBase
    {
        internal static string TextOf(TextBlock textBlock) => new TextRange(textBlock.ContentStart, textBlock.ContentEnd).Text;

        internal static string[] BoldTextsOf(TextBlock textBlock)
        {
            return Descendants(textBlock.Inlines)
                .OfType<Bold>()
                .Select(p => new TextRange(p.ContentStart, p.ContentEnd).Text)
                .ToArray();

            // formatting is applied by nesting, e.g. <i><b>text</b></i> becomes Italic(Bold(Run))
            static IEnumerable<Inline> Descendants(InlineCollection inlines)
            {
                foreach (var inline in inlines)
                {
                    yield return inline;

                    if (inline is Span span)
                    {
                        foreach (var child in Descendants(span.Inlines))
                        {
                            yield return child;
                        }
                    }
                }
            }
        }

        [Fact]
        public Task Should_Render_Text_And_Formatting()
        {
            return StaRunner.Run(() =>
            {
                var textBlock = new HtmlTextBlock { Html = "plain <b>bold</b>" };

                Assert.Multiple(() =>
                {
                    Assert.Equal("plain bold", TextOf(textBlock));
                    Assert.Equal(["bold"], BoldTextsOf(textBlock));
                });

                return Task.CompletedTask;
            });
        }

        [Theory]
        [InlineData("x <!-- a > b --> y", "x  y")]
        [InlineData("x <!-- > --> y", "x  y")]
        [InlineData("x <!-- comment --> y", "x  y")]
        public Task Should_Skip_Comments(string html, string expected)
        {
            return StaRunner.Run(() =>
            {
                var textBlock = new HtmlTextBlock { Html = html };

                Assert.Equal(expected, TextOf(textBlock));

                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task Setting_Null_Should_Clear_The_Text()
        {
            return StaRunner.Run(() =>
            {
                var textBlock = new HtmlTextBlock { Html = "text" };

                textBlock.Html = null!;

                Assert.Empty(textBlock.Inlines);

                return Task.CompletedTask;
            });
        }
    }

    public sealed class HtmlHighlightTextBlockTests : TraceTestBase
    {
        [Fact]
        public Task Should_Highlight_Matches_In_Bold_Ignoring_Case()
        {
            return StaRunner.Run(() =>
            {
                var textBlock = new HtmlHighlightTextBlock { Highlight = "WORLD", Html = "hello world" };

                Assert.Multiple(() =>
                {
                    Assert.Equal("hello world", HtmlTextBlockTests.TextOf(textBlock));
                    Assert.Equal(["world"], HtmlTextBlockTests.BoldTextsOf(textBlock));
                });

                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task Should_Highlight_Every_Match()
        {
            return StaRunner.Run(() =>
            {
                var textBlock = new HtmlHighlightTextBlock { Highlight = "a", Html = "banana" };

                Assert.Multiple(() =>
                {
                    Assert.Equal("banana", HtmlTextBlockTests.TextOf(textBlock));
                    Assert.Equal(["a", "a", "a"], HtmlTextBlockTests.BoldTextsOf(textBlock));
                });

                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task Should_Not_Highlight_Inside_Tags()
        {
            return StaRunner.Run(() =>
            {
                var textBlock = new HtmlHighlightTextBlock { Highlight = "b", Html = "<i>abc</i>" };

                Assert.Multiple(() =>
                {
                    Assert.Equal("abc", HtmlTextBlockTests.TextOf(textBlock));
                    Assert.Equal(["b"], HtmlTextBlockTests.BoldTextsOf(textBlock));
                });

                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task Should_Treat_Highlight_As_Literal_Text()
        {
            return StaRunner.Run(() =>
            {
                var textBlock = new HtmlHighlightTextBlock { Highlight = "a.c", Html = "abc a.c" };

                Assert.Equal(["a.c"], HtmlTextBlockTests.BoldTextsOf(textBlock));

                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task Changing_Highlight_Should_Render_Again()
        {
            return StaRunner.Run(() =>
            {
                var textBlock = new HtmlHighlightTextBlock { Html = "abc" };
                Assert.Empty(HtmlTextBlockTests.BoldTextsOf(textBlock));

                textBlock.Highlight = "b";

                Assert.Multiple(() =>
                {
                    Assert.Equal("abc", HtmlTextBlockTests.TextOf(textBlock));
                    Assert.Equal(["b"], HtmlTextBlockTests.BoldTextsOf(textBlock));
                });

                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task Should_Tolerate_Null_Html()
        {
            return StaRunner.Run(() =>
            {
                var textBlock = new HtmlHighlightTextBlock { Highlight = "b", Html = "abc" };

                textBlock.Html = null!;

                Assert.Empty(textBlock.Inlines);

                return Task.CompletedTask;
            });
        }

        [Theory]
        [InlineData("en-US")]
        [InlineData("tr-TR")]
        public Task Uppercase_Tags_Should_Be_Recognized_Independent_Of_Culture(string cultureName)
        {
            return StaRunner.Run(() =>
            {
                var culture = CultureInfo.CurrentCulture;
                try
                {
                    // in turkish "I".ToLower() is "ı", so <I> would not be recognized as italic
                    CultureInfo.CurrentCulture = new CultureInfo(cultureName);

                    var textBlock = new HtmlTextBlock { Html = "<I>italic</I> <B>bold</B>" };
                    var inlines = Descendants(textBlock.Inlines).ToArray();

                    Assert.Multiple(() =>
                    {
                        Assert.Equal("italic bold", HtmlTextBlockTests.TextOf(textBlock));
                        Assert.Single(inlines.OfType<Italic>());
                        Assert.Equal(["bold"], HtmlTextBlockTests.BoldTextsOf(textBlock));
                    });
                }
                finally
                {
                    CultureInfo.CurrentCulture = culture;
                }

                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task Text_After_The_Last_Tag_Should_Not_Repeat_That_Tag()
        {
            return StaRunner.Run(() =>
            {
                // the previous parser kept the last tag name and added it again after trailing text
                var textBlock = new HtmlTextBlock { Html = "a<br>b" };

                Assert.Single(Descendants(textBlock.Inlines).OfType<LineBreak>());

                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task Text_Between_Two_Comments_Should_Be_Kept()
        {
            return StaRunner.Run(() =>
            {
                // the previous parser skipped from the first comment to the end of the second one
                var textBlock = new HtmlTextBlock { Html = "a<!-- one -->b<b>c</b><!-- two -->d" };

                Assert.Multiple(() =>
                {
                    Assert.Equal("abcd", HtmlTextBlockTests.TextOf(textBlock));
                    Assert.Equal(["c"], HtmlTextBlockTests.BoldTextsOf(textBlock));
                });

                return Task.CompletedTask;
            });
        }

        [Theory]
        [InlineData("a <b>b</b> c", "a b c")]
        [InlineData("<b>a</b>", "a")]
        [InlineData("a < b", "a < b")]
        [InlineData("a <> b", "a  b")]
        [InlineData("a < b> c", "a  c")]
        [InlineData("<unknown>a</unknown>", "a")]
        [InlineData("<!-- unterminated", "<!-- unterminated")] // without a closing ">" there is no tag, so it stays text
        [InlineData("a <!-- x -- y --> b", "a  b")]
        public Task Should_Scan_Edge_Cases(string html, string expected)
        {
            return StaRunner.Run(() =>
            {
                var textBlock = new HtmlTextBlock { Html = html };

                Assert.Equal(expected, HtmlTextBlockTests.TextOf(textBlock));

                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task Should_Parse_Large_Html_In_Linear_Time()
        {
            return StaRunner.Run(() =>
            {
                // 20000 tags took about 6 seconds with the previous parser, which copied the rest of the input for every tag
                var html = string.Concat(Enumerable.Repeat("text <b>bold</b> and <i>italic</i> ", 5_000));

                var watch = System.Diagnostics.Stopwatch.StartNew();
                var textBlock = new HtmlTextBlock { Html = html };
                watch.Stop();

                Assert.Equal(5_000, HtmlTextBlockTests.BoldTextsOf(textBlock).Length);
                Assert.True(watch.Elapsed < TimeSpan.FromSeconds(3), $"parsing took {watch.Elapsed}");

                return Task.CompletedTask;
            });
        }

        private static IEnumerable<Inline> Descendants(InlineCollection inlines)
        {
            foreach (var inline in inlines)
            {
                yield return inline;

                if (inline is Span span)
                {
                    foreach (var child in Descendants(span.Inlines))
                    {
                        yield return child;
                    }
                }
            }
        }
    }
}
