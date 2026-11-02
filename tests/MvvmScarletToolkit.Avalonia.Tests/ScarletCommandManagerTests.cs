using Avalonia.Threading;
using MvvmScarletToolkit.Avalonia.Tests.Util;

namespace MvvmScarletToolkit.Avalonia.Tests
{
    public sealed class ScarletCommandManagerTests : TraceTestBase
    {
        private readonly AvaloniaPlatformFixture _platform;

        // TestContext.Current does not flow to the Avalonia UI thread, capture what is needed up front
        private readonly CancellationToken _cancellationToken = TestContext.Current.CancellationToken;

        public ScarletCommandManagerTests(AvaloniaPlatformFixture platform)
        {
            _platform = platform;
        }

        [Fact]
        public Task InvalidateRequerySuggested_Raises_Event_On_UI_Thread_When_Called_From_Background_Thread()
        {
            return _platform.RunOnUIThread(async () =>
            {
                var manager = new ScarletCommandManager();
                var wasOnUIThread = false;
                manager.RequerySuggested += (_, _) => wasOnUIThread = Dispatcher.UIThread.CheckAccess();

                await Task.Run(manager.InvalidateRequerySuggested, _cancellationToken);
                Dispatcher.UIThread.RunJobs();

                Assert.True(wasOnUIThread);
            });
        }

        [Fact]
        public Task InvalidateRequerySuggested_Raises_Single_Event_For_Calls_In_A_Row()
        {
            return _platform.RunOnUIThread(() =>
            {
                var manager = new ScarletCommandManager();
                var count = 0;
                manager.RequerySuggested += (_, _) => count++;

                manager.InvalidateRequerySuggested();
                manager.InvalidateRequerySuggested();
                Dispatcher.UIThread.RunJobs();

                Assert.Equal(1, count);

                manager.InvalidateRequerySuggested();
                Dispatcher.UIThread.RunJobs();

                Assert.Equal(2, count);

                return Task.CompletedTask;
            });
        }
    }
}
