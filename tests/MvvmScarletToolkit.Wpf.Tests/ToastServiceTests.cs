using MvvmScarletToolkit.Wpf.Tests.Util;

namespace MvvmScarletToolkit.Wpf.Tests
{
    public sealed class ToastServiceTests : TraceTestBase
    {
        private static readonly ToastServiceConfiguration _configuration = new()
        {
            ToastVisibleFor = TimeSpan.FromMilliseconds(20),
            ToastCloseDelay = TimeSpan.FromMilliseconds(10),
            WindowCloseDelay = TimeSpan.FromMilliseconds(10),
        };

        /// <summary>
        /// Does not show a host window, so that tests don't need an Application and don't flash windows on screen.
        /// </summary>
        private class HeadlessToastService(ToastServiceConfiguration configuration, SynchronizationContext synchronizationContext)
            : ToastService(configuration, synchronizationContext)
        {
            protected override void SetupHost()
            {
            }
        }

        private sealed class FailingToastService(ToastServiceConfiguration configuration, SynchronizationContext synchronizationContext)
            : HeadlessToastService(configuration, synchronizationContext)
        {
            public int Attempts;

            protected override Task CloseToast(IToast toast)
            {
                Interlocked.Increment(ref Attempts);
                throw new InvalidOperationException("closing failed");
            }
        }

        [Fact]
        public void Default_Should_Not_Cache_A_Failed_Creation()
        {
            // no Application in the test process. An exception cached by Lazy would be rethrown as the same instance
            var first = Record.Exception(() => ToastService.Default);
            var second = Record.Exception(() => ToastService.Default);

            Assert.Multiple(() =>
            {
                Assert.IsType<InvalidOperationException>(first);
                Assert.IsType<InvalidOperationException>(second);
                Assert.NotSame(first, second);
            });
        }

        [Fact]
        public Task Should_Remove_Toasts_After_They_Have_Been_Visible()
        {
            return StaRunner.Run(async () =>
            {
                // visible long enough to observe the toast before it is removed again
                var configuration = new ToastServiceConfiguration
                {
                    ToastVisibleFor = TimeSpan.FromMilliseconds(300),
                    ToastCloseDelay = TimeSpan.FromMilliseconds(10),
                    WindowCloseDelay = TimeSpan.FromMilliseconds(10),
                };
                using var service = new HeadlessToastService(configuration, SynchronizationContext.Current!);
                var toast = new ToastViewModel("title", "body", ToastType.Information, false);

                service.Show(toast);
                await Ui.WaitUntil(() => service.Items.Count == 1);

                await Ui.WaitUntil(() => service.Items.Count == 0);
                Assert.True(toast.IsRemoving);
            });
        }

        [Fact]
        public Task Should_Keep_Persistent_Toasts()
        {
            return StaRunner.Run(async () =>
            {
                using var service = new HeadlessToastService(_configuration, SynchronizationContext.Current!);
                var toast = new ToastViewModel("title", "body", ToastType.Information, true);

                service.Show(toast);
                await Ui.WaitUntil(() => service.Items.Count == 1);
                await Task.Delay(100);
                await Ui.Settle();

                Assert.Same(toast, Assert.Single(service.Items));
                Assert.False(toast.IsRemoving);
            });
        }

        [Fact]
        public Task Failing_To_Close_A_Toast_Should_Not_Crash()
        {
            // the closing used to run in an async void lambda on the thread pool, an exception there terminates the process
            return StaRunner.Run(async () =>
            {
                using var service = new FailingToastService(_configuration, SynchronizationContext.Current!);

                service.Show(new ToastViewModel("title", "body", ToastType.Error, false));

                await Ui.WaitUntil(() => Volatile.Read(ref service.Attempts) == 1);
                await Task.Delay(50);
                await Ui.Settle();

                Assert.Single(service.Items);
            });
        }
    }
}
