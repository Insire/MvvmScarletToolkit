namespace MvvmScarletToolkit.Wpf.Tests
{
    public sealed class ShellExecuteTests : TraceTestBase
    {
        [Fact]
        public void Should_Pass_The_Whole_Url_To_The_Shell()
        {
            const string url = "https://example.com/search?a=1&b=2&calc";

            var info = ShellExecute.CreateStartInfo(url);

            Assert.Multiple(() =>
            {
                Assert.True(info.UseShellExecute);
                Assert.Equal(url, info.FileName);
                Assert.Empty(info.Arguments);
            });
        }

        [Fact]
        public void Should_Not_Go_Through_Cmd()
        {
            var info = ShellExecute.CreateStartInfo(new Uri("https://example.com/?a=1&b=2"));

            Assert.NotEqual("cmd", info.FileName, StringComparer.OrdinalIgnoreCase);
        }

        [Fact]
        public void Should_Use_The_Absolute_Uri()
        {
            var info = ShellExecute.CreateStartInfo(new Uri("https://example.com/a b?x=1&y=2"));

            Assert.Equal("https://example.com/a%20b?x=1&y=2", info.FileName);
        }

        [Fact]
        public void Should_Keep_Relative_Uris_As_Written()
        {
            var info = ShellExecute.CreateStartInfo(new Uri("docs/readme.md", UriKind.Relative));

            Assert.Equal("docs/readme.md", info.FileName);
        }
    }
}
