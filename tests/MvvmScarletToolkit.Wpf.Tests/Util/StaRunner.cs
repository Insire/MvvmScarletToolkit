using System.Windows.Threading;

namespace MvvmScarletToolkit.Wpf.Tests.Util
{
    /// <summary>
    /// Runs a test on a dedicated STA thread with a pumping WPF dispatcher,
    /// and fails it when the dispatcher observes an unhandled exception (e.g. from an async void handler).
    /// </summary>
    internal static class StaRunner
    {
        public static Task Run(Func<Task> test)
        {
            var result = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var thread = new Thread(() =>
            {
                var dispatcher = Dispatcher.CurrentDispatcher;
                SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext(dispatcher));

                Exception? failure = null;
                dispatcher.UnhandledException += (_, e) =>
                {
                    failure ??= e.Exception;
                    e.Handled = true;
                };

                var frame = new DispatcherFrame();
                dispatcher.InvokeAsync(async () =>
                {
                    try
                    {
                        await test();

                        // let queued work, like continuations of async void handlers, run before finishing
                        await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
                    }
                    catch (Exception ex)
                    {
                        failure ??= ex;
                    }
                    finally
                    {
                        frame.Continue = false;
                    }
                });

                Dispatcher.PushFrame(frame);
                dispatcher.InvokeShutdown();

                if (failure is null)
                {
                    result.SetResult();
                }
                else
                {
                    result.SetException(failure);
                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.IsBackground = true;
            thread.Start();

            return result.Task;
        }
    }
}
