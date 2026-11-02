using MvvmScarletToolkit.Wpf.Features.FileSystemBrowser;
using System.IO;
using System.Text;

namespace MvvmScarletToolkit.Wpf.Tests
{
    public sealed class FileExtensionsTests : TraceTestBase
    {
        [Theory]
        [InlineData("", 0)]
        [InlineData("a", 1)]
        [InlineData("\n", 1)]
        [InlineData("a\n", 1)]
        [InlineData("a\nb", 2)]
        [InlineData("a\nb\n", 2)]
        [InlineData("a\r\nb\r\n", 2)]
        [InlineData("a\r\nb", 2)]
        [InlineData("a\rb\r", 2)]
        [InlineData("\n\n\n\n\n\n", 6)]
        [InlineData("aaaa\nbbbb\ncccc", 3)]
        [InlineData("aaaaaaaaaaaaaaaaa\nbbbbbbbbbbbbbbbbb\nc", 3)]
        public void CountLinesMaybe_Should_Count_Lines(string content, long expected)
        {
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

            Assert.Equal(expected, stream.CountLinesMaybe());
        }

        [Fact]
        public void CountLinesMaybe_Should_Count_Across_Read_Buffers()
        {
            const int lines = 500_000;
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(string.Concat(Enumerable.Repeat("abcdefg\n", lines))));

            Assert.Equal(lines, stream.CountLinesMaybe());
        }
    }
}
