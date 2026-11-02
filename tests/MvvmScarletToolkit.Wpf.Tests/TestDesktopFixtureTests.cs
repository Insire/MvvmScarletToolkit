using MvvmScarletToolkit.Wpf.Tests.Util;

namespace MvvmScarletToolkit.Wpf.Tests
{
    public sealed class TestDesktopFixtureTests : TraceTestBase
    {
        [Fact]
        public Task Windows_Of_Tests_Should_Be_On_The_Hidden_Desktop()
        {
            return StaRunner.Run(async () =>
            {
                Assert.SkipUnless(TestDesktopFixture.IsAvailable, "The hidden desktop could not be created.");

                var window = Ui.Show("content");
                await Ui.WaitUntil(() => window.IsLoaded);

                Assert.True(TestDesktopFixture.IsCurrentThreadOnDesktop());
                window.Close();
            });
        }
    }
}
