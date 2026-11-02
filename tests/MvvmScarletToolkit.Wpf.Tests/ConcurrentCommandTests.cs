using MvvmScarletToolkit.Commands;
using MvvmScarletToolkit.Wpf.Tests.Util;

namespace MvvmScarletToolkit.Wpf.Tests
{
    public sealed class ConcurrentCommandTests : TraceTestBase, IAsyncLifetime
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        private static ScarletCommandBuilder _builder;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

        public ValueTask InitializeAsync()
        {
            _builder = new ScarletCommandBuilder(Utils.GetTestDispatcher(), Utils.GetTestCommandManager(), Utils.GetTestExceptionHandler(), Utils.GetTestMessenger(), Utils.GetTestExitService(), Utils.GetTestEventManager(), Utils.TestBusyStackFactory);

            return ValueTask.CompletedTask;
        }

        public ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }

        [Fact]
        public void Ctor_DoesNotThrow()
        {
            _ = new ConcurrentCommand<object>(Utils.GetTestCommandManager(), Utils.GetTestExceptionHandler(), Utils.GetTestCancelCommand(), Utils.GetTestBusyStackFactory(), (_, _) => Task.CompletedTask);
        }

        [Fact]
        public void Ctor_DoesThrowForNullCommandManager()
        {
            Assert.Throws<ArgumentNullException>(() => new ConcurrentCommand<object>(null!, Utils.GetTestExceptionHandler(), Utils.GetTestCancelCommand(), Utils.GetTestBusyStackFactory(), (_, _) => Task.CompletedTask));
        }

        [Fact]
        public void Ctor_DoesThrowForNullCancelCommand()
        {
            Assert.Throws<ArgumentNullException>(() => new ConcurrentCommand<object>(Utils.GetTestCommandManager(), Utils.GetTestExceptionHandler(), null!, Utils.GetTestBusyStackFactory(), (_, _) => Task.CompletedTask));
        }

        [Fact]
        public void Ctor_DoesThrowForNullBusyStackFactory()
        {
            Assert.Throws<NullReferenceException>(() => new ConcurrentCommand<object>(Utils.GetTestCommandManager(), Utils.GetTestExceptionHandler(), Utils.GetTestCancelCommand(), null!, (_, _) => Task.CompletedTask));
        }

        [Fact]
        public void Ctor_DoesThrowForNullBusyStack()
        {
            Assert.Throws<ArgumentNullException>(() => new ConcurrentCommand<object>(Utils.GetTestCommandManager(), Utils.GetTestExceptionHandler(), Utils.GetTestCancelCommand(), Utils.GetTestBusyStackFactory(), default!, (_, _) => Task.CompletedTask, _ => true));
        }

        [Fact]
        public void Ctor_DoesThrowForNullExecute()
        {
            Assert.Throws<ArgumentNullException>(() => new ConcurrentCommand<object>(Utils.GetTestCommandManager(), Utils.GetTestExceptionHandler(), Utils.GetTestCancelCommand(), Utils.GetTestBusyStackFactory(), null!));
        }

        [Fact]
        public void Ctor_DoesThrowForNullCanExecute()
        {
            Assert.Throws<ArgumentNullException>(() => new ConcurrentCommand<object>(Utils.GetTestCommandManager(), Utils.GetTestExceptionHandler(), Utils.GetTestCancelCommand(), Utils.GetTestBusyStackFactory(), (_, _) => Task.CompletedTask, null!));
        }

        [Fact]
        public void Execute_IsCalledOnCommandExecution()
        {
            var executed = false;
            var command = _builder.Create((_) => { executed = true; return Task.CompletedTask; })
                .Build();

            command.Execute(default);

            Assert.True(executed);
        }

        [Fact]
        public void CanExecute_IsCalledOnCommandCanExecuteCheck()
        {
            var executed = false;
            var command = _builder.Create((object _) => { Assert.Fail(); return Task.CompletedTask; }, (_) => { executed = true; return true; })
                .Build();

            command.CanExecute(default);

            Assert.True(executed);
        }

        [Fact]
        public void Execute_IsCalledOnCommandExecutionForValueType()
        {
            var executed = false;
            var command = _builder.Create((int _) => { executed = true; return Task.CompletedTask; })
                .Build();

            command.Execute(1);

            Assert.True(executed);
        }

        [Fact]
        public void Execute_IsStillCalledOnCommandExecutionForValueTypeAndNullArgument()
        {
            var executed = false;
            var command = _builder.Create((int _) => { executed = true; return Task.CompletedTask; })
                .Build();

            command.Execute(null);

            Assert.True(executed);
        }

        [Fact]
        public void CanExecute_IsCalledOnCommandCanExecuteCheckForValueTypeAndNullArgument()
        {
            var executed = false;
            var command = _builder.Create((int _) => { executed = true; return Task.CompletedTask; })
                .Build();

            command.CanExecute(null);

            Assert.False(executed);
        }

        [Fact]
        public void Execute_IsNotCalledForArgumentOfDifferentType()
        {
            var executed = false;
            var command = _builder.Create((int _) => { executed = true; return Task.CompletedTask; })
                .Build();

            command.Execute("1");

            Assert.False(executed);
        }

        [Fact]
        public async Task ExecuteAsync_RoutesSynchronousExceptionToExceptionHandler()
        {
            var exceptionHandler = Utils.GetTestExceptionHandler();
            var command = Utils.GetTestCommandBuilder(exceptionHandler: exceptionHandler)
                .Create((object? _, CancellationToken _) => throw new InvalidOperationException())
                .Build();

            await command.ExecuteAsync(null);

            await exceptionHandler.Received(1).Handle(Arg.Any<InvalidOperationException>());
            Assert.False(command.IsBusy);
        }

        [Fact]
        public async Task ExecuteAsync_DoesNotRouteCancellationToExceptionHandler()
        {
            var exceptionHandler = Utils.GetTestExceptionHandler();
            var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var command = (ConcurrentCommand<object?>)Utils.GetTestCommandBuilder(exceptionHandler: exceptionHandler)
                .Create(async (object? _, CancellationToken token) =>
                {
                    started.SetResult();
                    await Task.Delay(Timeout.Infinite, token);
                })
                .WithCancellation()
                .Build();

            var execution = command.ExecuteAsync(null);
            await started.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

            command.CancelCommand!.Execute(null);
            await execution.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

            Assert.True(command.Execution.IsCanceled);
            await exceptionHandler.DidNotReceive().Handle(Arg.Any<Exception>());
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task ExecuteAsync_WhileBusy_CancelsOnlyThePreviousExecution(bool asyncCancellation)
        {
            var firstStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var secondRelease = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var tokens = new List<CancellationToken>();

            var context = Utils.GetTestCommandBuilder()
                .Create(async (object? _, CancellationToken token) =>
                {
                    tokens.Add(token);
                    if (tokens.Count == 1)
                    {
                        firstStarted.SetResult();
                        await Task.Delay(Timeout.Infinite, token);
                    }
                    else
                    {
                        await secondRelease.Task;
                    }
                });

            var command = (asyncCancellation ? context.WithAsyncCancellation() : context.WithCancellation()).Build();

            var first = command.ExecuteAsync(null);
            await firstStarted.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

            var second = command.ExecuteAsync(null);
            await first.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

            Assert.Equal(2, tokens.Count);
            Assert.True(tokens[0].IsCancellationRequested);
            Assert.False(tokens[1].IsCancellationRequested);

            // the second execution is still running, so it has to stay busy and cancellable
            Assert.True(command.IsBusy);
            Assert.True(command.CancelCommand!.CanExecute(null));

            secondRelease.SetResult();
            await second.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

            Assert.False(command.IsBusy);
            Assert.False(command.CancelCommand.CanExecute(null));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task CancelCommand_CancelsRunningExecution(bool asyncCancellation)
        {
            var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var context = Utils.GetTestCommandBuilder()
                .Create(async (object? _, CancellationToken token) =>
                {
                    started.SetResult();
                    await Task.Delay(Timeout.Infinite, token);
                });

            var command = (asyncCancellation ? context.WithAsyncCancellation() : context.WithCancellation()).Build();

            var execution = command.ExecuteAsync(null);
            await started.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

            Assert.True(command.CancelCommand!.CanExecute(null));
            command.CancelCommand.Execute(null);

            // a source that was cancelled can't be cancelled again
            Assert.False(command.CancelCommand.CanExecute(null));

            await execution.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

            Assert.False(command.IsBusy);
            Assert.False(command.CancelCommand.CanExecute(null));
        }

        [Fact]
        public async Task ExecuteAsync_InvalidatesRequerySuggestedAfterUpdatingCompletion()
        {
            var commandManager = Utils.GetTestCommandManager();
            var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var command = Utils.GetTestCommandBuilder(commandManager: commandManager)
                .Create(async (object? _, CancellationToken _) => await release.Task)
                .Build();

            var completionWasRunning = new List<bool>();
            commandManager.When(p => p.InvalidateRequerySuggested()).Do(_ => completionWasRunning.Add(command.Completion?.IsCompleted == false));

            var execution = command.ExecuteAsync(null);
            release.SetResult();
            await execution.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

            // once when starting, once when finishing
            Assert.Equal(new[] { true, false }, completionWasRunning);
        }
    }
}
