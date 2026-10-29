using MvvmScarletToolkit.Wpf.Tests.Util;
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
    }
}
