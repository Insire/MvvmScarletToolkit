using MvvmScarletToolkit.Commands;
using MvvmScarletToolkit.Wpf.Tests.Util;

namespace MvvmScarletToolkit.Wpf.Tests
{
    public sealed class SequentialAsyncCommandDecoratorTests : TraceTestBase
    {
        [Fact]
        public async Task ExecuteAsync_SkipsExecutionWhileRunning()
        {
            var executions = 0;
            var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var command = Utils.GetTestCommandBuilder()
                .Create(async (object? _, CancellationToken _) =>
                {
                    executions++;
                    await release.Task;
                })
                .WithSingleExecution()
                .Build();

            Assert.True(command.CanExecute(null));

            var first = command.ExecuteAsync(null);

            Assert.False(command.CanExecute(null));

            // has to return right away instead of waiting for the running execution
            await command.ExecuteAsync(null).WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

            release.SetResult();
            await first.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

            Assert.Equal(1, executions);
            Assert.True(command.CanExecute(null));
        }

        [Fact]
        public async Task ExecuteAsync_RunsCommandWithoutCompletion()
        {
            var inner = new CommandWithoutCompletion(Utils.GetTestCommandManager());
            var command = new SequentialAsyncCommandDecorator(Utils.GetTestCommandManager(), inner, null);

            Assert.True(command.CanExecute(null));

            await command.ExecuteAsync(null);

            Assert.True(inner.Executed);
        }

        [Fact]
        public void Name_UsesProvidedId()
        {
            var command = new SequentialAsyncCommandDecorator(Utils.GetTestCommandManager(), new CommandWithoutCompletion(Utils.GetTestCommandManager()), "test");

            Assert.Equal("test", command.Name);
        }

        [Fact]
        public void Name_IsGeneratedOnceWithoutId()
        {
            var command = new SequentialAsyncCommandDecorator(Utils.GetTestCommandManager(), new CommandWithoutCompletion(Utils.GetTestCommandManager()), null);

            Assert.False(string.IsNullOrEmpty(command.Name));
            Assert.Same(command.Name, command.Name);
        }

        private sealed class CommandWithoutCompletion : ConcurrentCommandBase
        {
            public bool Executed { get; private set; }

#pragma warning disable RCS1210 // Return completed task instead of returning null - a null completion is the scenario under test
            public override Task? Completion => null;
#pragma warning restore RCS1210

            public CommandWithoutCompletion(IScarletCommandManager commandManager)
                : base(commandManager)
            {
            }

            public override bool CanExecute(object? parameter)
            {
                return true;
            }

            public override void Execute(object? parameter)
            {
                Executed = true;
            }

            public override Task ExecuteAsync(object? parameter)
            {
                Executed = true;
                return Task.CompletedTask;
            }
        }
    }
}
