using MvvmScarletToolkit.Commands;
using MvvmScarletToolkit.Wpf.Tests.Util;

namespace MvvmScarletToolkit.Wpf.Tests
{
    public sealed class NotifyTaskCompletionTests : TraceTestBase
    {
        [Fact]
        public async Task CanceledTask_IsNotRoutedToExceptionHandler()
        {
            var exceptionHandler = Utils.GetTestExceptionHandler();
            var completion = new NotifyTaskCompletion(Task.FromCanceled(new CancellationToken(true)), exceptionHandler);

            await completion.TaskCompletion;

            Assert.True(completion.IsCanceled);
            await exceptionHandler.DidNotReceive().Handle(Arg.Any<Exception>());
        }

        [Fact]
        public async Task FaultedTask_IsRoutedToExceptionHandler()
        {
            var exceptionHandler = Utils.GetTestExceptionHandler();
            var completion = new NotifyTaskCompletion(Task.FromException(new InvalidOperationException("failed")), exceptionHandler);

            await completion.TaskCompletion;

            Assert.True(completion.IsFaulted);
            Assert.Equal("failed", completion.ErrorMessage);
            await exceptionHandler.Received(1).Handle(Arg.Any<InvalidOperationException>());
        }

        [Fact]
        public async Task Exception_IsOnlyFlattenedOnce()
        {
            var completion = new NotifyTaskCompletion(Task.FromException(new InvalidOperationException()), Utils.GetTestExceptionHandler());

            await completion.TaskCompletion;

            Assert.NotNull(completion.Exception);
            Assert.Same(completion.Exception, completion.Exception);
        }

        [Fact]
        public async Task CompletedTask_RaisesSuccessNotifications()
        {
            var source = new TaskCompletionSource();
            var completion = new NotifyTaskCompletion(source.Task, Utils.GetTestExceptionHandler());
            var properties = new List<string?>();
            completion.PropertyChanged += (_, e) => properties.Add(e.PropertyName);

            source.SetResult();
            await completion.TaskCompletion;

            Assert.Equal(new[] { nameof(NotifyTaskCompletion.Status), nameof(NotifyTaskCompletion.IsRunning), nameof(NotifyTaskCompletion.IsCompleted), nameof(NotifyTaskCompletion.IsSuccessfullyCompleted) }, properties);
        }
    }
}
