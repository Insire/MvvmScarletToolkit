using MvvmScarletToolkit.Commands;
using MvvmScarletToolkit.Wpf.Tests.Util;

namespace MvvmScarletToolkit.Wpf.Tests
{
    public sealed class DisabledReasonTests : TraceTestBase
    {
        private const string Reason = "Add at least one item";

        [Fact]
        public void DisabledReason_IsSetWhileDisabledAndClearedWhenEnabled()
        {
            var canExecute = false;
            var command = Utils.GetTestCommandBuilder()
                .Create(() => Task.CompletedTask, () => canExecute)
                .WithDisabledReason(() => Reason)
                .Build();

            Assert.False(command.CanExecute(null));
            Assert.Equal(Reason, command.DisabledReason);

            canExecute = true;

            Assert.True(command.CanExecute(null));
            Assert.Null(command.DisabledReason);
        }

        [Fact]
        public void DisabledReason_IsNullWithoutEvaluator()
        {
            var command = Utils.GetTestCommandBuilder()
                .Create(() => Task.CompletedTask, () => false)
                .Build();

            Assert.False(command.CanExecute(null));
            Assert.Null(command.DisabledReason);
        }

        [Fact]
        public void DisabledReason_EvaluatorIsNotInvokedWhileEnabled()
        {
            var evaluations = 0;
            var command = Utils.GetTestCommandBuilder()
                .Create(() => Task.CompletedTask, () => true)
                .WithDisabledReason(() =>
                {
                    evaluations++;
                    return Reason;
                })
                .Build();

            Assert.True(command.CanExecute(null));
            Assert.Equal(0, evaluations);
        }

        [Fact]
        public void DisabledReason_ReceivesTypedArgument()
        {
            var command = Utils.GetTestCommandBuilder()
                .Create<string>((_, _) => Task.CompletedTask, path => !string.IsNullOrEmpty(path))
                .WithDisabledReason(path => path is null ? "No path" : "Empty path")
                .Build();

            Assert.False(command.CanExecute(null));
            Assert.Equal("No path", command.DisabledReason);

            Assert.False(command.CanExecute(string.Empty));
            Assert.Equal("Empty path", command.DisabledReason);

            Assert.True(command.CanExecute("file.txt"));
            Assert.Null(command.DisabledReason);
        }

        [Fact]
        public void DisabledReason_IsNullForArgumentOfWrongType()
        {
            var evaluations = 0;
            var command = Utils.GetTestCommandBuilder()
                .Create<string>((_, _) => Task.CompletedTask, _ => false)
                .WithDisabledReason(_ =>
                {
                    evaluations++;
                    return Reason;
                })
                .Build();

            Assert.False(command.CanExecute(42));
            Assert.Null(command.DisabledReason);
            Assert.Equal(0, evaluations);
        }

        [Fact]
        public void DisabledReason_RaisesPropertyChangedOnlyOnChange()
        {
            var canExecute = false;
            var command = Utils.GetTestCommandBuilder()
                .Create(() => Task.CompletedTask, () => canExecute)
                .WithDisabledReason(() => Reason)
                .Build();

            var notifications = 0;
            command.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(ConcurrentCommandBase.DisabledReason))
                {
                    notifications++;
                }
            };

            command.CanExecute(null);
            command.CanExecute(null);
            Assert.Equal(1, notifications);

            canExecute = true;
            command.CanExecute(null);
            command.CanExecute(null);
            Assert.Equal(2, notifications);
        }

        [Fact]
        public void WithDisabledReason_LastCallWins()
        {
            var command = Utils.GetTestCommandBuilder()
                .Create(() => Task.CompletedTask, () => false)
                .WithDisabledReason(() => "first")
                .WithDisabledReason(() => "second")
                .Build();

            command.CanExecute(null);

            Assert.Equal("second", command.DisabledReason);
        }

        [Fact]
        public void WithDisabledReason_ThrowsForNullEvaluator()
        {
            var context = Utils.GetTestCommandBuilder().Create(() => Task.CompletedTask);

            Assert.Throws<ArgumentNullException>(() => context.WithDisabledReason((Func<string?>)null!));
            Assert.Throws<ArgumentNullException>(() => context.WithDisabledReason((Func<object?, string?>)null!));
        }

        [Fact]
        public void WithDisabledReason_ThrowsForNullContext()
        {
            Assert.Throws<ArgumentNullException>(() => ((CommandBuilderContext<object?>)null!).WithDisabledReason(() => Reason));
        }

        [Fact]
        public void IConcurrentCommand_ExposesDisabledReason()
        {
            IConcurrentCommand command = Utils.GetTestCommandBuilder()
                .Create(() => Task.CompletedTask, () => false)
                .WithDisabledReason(() => Reason)
                .Build();

            command.CanExecute(null!);

            Assert.Equal(Reason, command.DisabledReason);
        }

        [Fact]
        public void SingleExecution_PassesInnerReasonThroughWhileIdle()
        {
            var command = Utils.GetTestCommandBuilder()
                .Create(() => Task.CompletedTask, () => false)
                .WithDisabledReason(() => Reason)
                .WithSingleExecution()
                .Build();

            Assert.False(command.CanExecute(null));
            Assert.Equal(Reason, command.DisabledReason);
        }

        [Fact]
        public async Task SingleExecution_ReportsDefaultTextWhileRunning()
        {
            var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var command = Utils.GetTestCommandBuilder()
                .Create(() => release.Task)
                .WithSingleExecution()
                .Build();

            var running = command.ExecuteAsync(null);

            Assert.False(command.CanExecute(null));
            Assert.Equal("Already running", command.DisabledReason);

            release.SetResult();
            await running.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

            Assert.True(command.CanExecute(null));
            Assert.Null(command.DisabledReason);
        }

        [Fact]
        public async Task SingleExecution_ReportsCustomTextWhileRunning()
        {
            var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var command = Utils.GetTestCommandBuilder()
                .Create(() => release.Task)
                .WithSingleExecution("save", "Saving...")
                .Build();

            var running = command.ExecuteAsync(null);

            Assert.Equal("Saving...", command.DisabledReason);

            release.SetResult();
            await running.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        }

        [Fact]
        public async Task SingleExecution_ReportsNothingWhileRunningWhenTextIsNull()
        {
            var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var command = Utils.GetTestCommandBuilder()
                .Create(() => release.Task)
                .WithSingleExecution(null, null)
                .Build();

            var running = command.ExecuteAsync(null);

            Assert.False(command.CanExecute(null));
            Assert.Null(command.DisabledReason);

            release.SetResult();
            await running.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        }

        [Fact]
        public async Task SingleExecution_RaisesPropertyChangedWhenRunningStateChanges()
        {
            var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var command = Utils.GetTestCommandBuilder()
                .Create(() => release.Task)
                .WithSingleExecution()
                .Build();

            var reasons = new List<string?>();
            command.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(ConcurrentCommandBase.DisabledReason))
                {
                    reasons.Add(((ConcurrentCommandBase)command).DisabledReason);
                }
            };

            var running = command.ExecuteAsync(null);

            Assert.Contains("Already running", reasons);

            release.SetResult();
            await running.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

            Assert.Null(reasons[^1]);
        }
    }
}
