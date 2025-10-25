namespace MvvmScarletToolkit.Tests
{
    public sealed class CommandBuilderExtensionTests : TraceTestBase, IAsyncLifetime
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        private static ScarletCommandBuilder _builder;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

        public ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }

        public ValueTask InitializeAsync()
        {
            _builder = new ScarletCommandBuilder(Utils.GetTestDispatcher(), Utils.GetTestCommandManager(), Utils.GetTestExceptionHandler(), Utils.GetTestMessenger(), Utils.GetTestExitService(), Utils.GetTestEventManager(), Utils.TestBusyStackFactory);

            return ValueTask.CompletedTask;
        }

        [Fact]
        public void Create_DoesReturnNewInstanceEveryTime()
        {
            var context = _builder.Create(() => Task.CompletedTask);
            Assert.NotNull(context);

            var newContext = _builder.Create(() => Task.CompletedTask);
            Assert.Multiple(() =>
            {
                Assert.NotNull(newContext);

                Assert.False(ReferenceEquals(context, newContext));
            });
        }

        [Fact]
        public void Create_DoesReturnValidContextInstance()
        {
            // default object overloads:

            var context = _builder.Create(() => Task.CompletedTask);
            Assert.NotNull(context);

            context = _builder.Create(() => Task.CompletedTask, () => true);
            Assert.NotNull(context);

            context = _builder.Create((CancellationToken _) => Task.CompletedTask);
            Assert.NotNull(context);

            context = _builder.Create((CancellationToken _) => Task.CompletedTask, () => true);
            Assert.NotNull(context);

            // generic overloads ignoring argument:

            context = _builder.Create<object?>(() => Task.CompletedTask);
            Assert.NotNull(context);

            context = _builder.Create<object?>(() => Task.CompletedTask, () => true);
            Assert.NotNull(context);

            context = _builder.Create<object?>((CancellationToken _) => Task.CompletedTask);
            Assert.NotNull(context);

            context = _builder.Create<object?>((CancellationToken _) => Task.CompletedTask, () => true);
            Assert.NotNull(context);

            // generic overloads with argument:

            context = _builder.Create((object? _) => Task.CompletedTask);
            Assert.NotNull(context);

            context = _builder.Create((object? _) => Task.CompletedTask, (object? _) => true);
            Assert.NotNull(context);

            context = _builder.Create((object? _) => Task.CompletedTask, () => true);
            Assert.NotNull(context);

            context = _builder.Create((object? _, CancellationToken __) => Task.CompletedTask);
            Assert.NotNull(context);

            context = _builder.Create((object? _, CancellationToken __) => Task.CompletedTask, (object? _) => true);
            Assert.NotNull(context);
        }
    }
}
