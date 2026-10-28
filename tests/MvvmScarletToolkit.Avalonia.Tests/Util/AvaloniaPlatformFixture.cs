using Avalonia;
using Avalonia.Headless;
using Avalonia.Threading;

namespace MvvmScarletToolkit.Avalonia.Tests.Util
{
    /// <summary>
    /// Sets up headless Avalonia with Skia once per test run. Bitmaps need its render interface,
    /// controls need its UI thread, use <see cref="RunOnUIThread"/> for those.
    /// </summary>
    public sealed class AvaloniaPlatformFixture : IAsyncLifetime
    {
        private readonly HeadlessUnitTestSession _session = HeadlessUnitTestSession.StartNew(typeof(AvaloniaPlatformFixture), AvaloniaTestIsolationLevel.PerAssembly);

        public static AppBuilder BuildAvaloniaApp()
        {
            return AppBuilder.Configure<Application>()
                .UseSkia()
                .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false });
        }

        public async ValueTask InitializeAsync()
        {
            // the platform is set up lazily on the first dispatch
            await _session.Dispatch(() => { }, CancellationToken.None);
        }

        public ValueTask DisposeAsync()
        {
            _session.Dispose();

            return ValueTask.CompletedTask;
        }

        /// <summary>
        /// Runs the test on the UI thread and fails it when the dispatcher observes an unhandled exception (e.g. from an async void handler).
        /// </summary>
        public Task RunOnUIThread(Func<Task> test)
        {
            return _session.Dispatch(async () =>
            {
                Exception? failure = null;
                void OnUnhandledException(object? sender, DispatcherUnhandledExceptionEventArgs e)
                {
                    failure ??= e.Exception;
                    e.Handled = true;
                }

                Dispatcher.UIThread.UnhandledException += OnUnhandledException;
                try
                {
                    await test();

                    // let queued work, like continuations of async void handlers, run before finishing
                    Dispatcher.UIThread.RunJobs();
                }
                finally
                {
                    Dispatcher.UIThread.UnhandledException -= OnUnhandledException;
                }

                if (failure is not null)
                {
                    throw new InvalidOperationException("The dispatcher observed an unhandled exception", failure);
                }

                return true;
            }, TestContext.Current.CancellationToken);
        }
    }
}
