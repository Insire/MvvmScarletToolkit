using MvvmScarletToolkit.Commands;

namespace MvvmScarletToolkit.Tests
{
    [TraceTest]
    public sealed class CommandBuilderContextTests
    {
        [Fact]
        public void Ctor_DoesntThrow()
        {
            var _ = new CommandBuilderContext<object>(Utils.GetTestCommandManager(), Utils.GetTestExceptionHandler(), Utils.TestBusyStackFactory, Utils.TestExecute, Utils.TestCanExecute);
        }

        [Fact]
        public void Ctor_ThrowsWithNullDependencies()
        {
            Assert.Throws<ArgumentNullException>(() => new CommandBuilderContext<object>(null!, Utils.GetTestExceptionHandler(), Utils.TestBusyStackFactory, Utils.TestExecute, Utils.TestCanExecute));
            Assert.Throws<ArgumentNullException>(() => new CommandBuilderContext<object>(Utils.GetTestCommandManager(), Utils.GetTestExceptionHandler(), null!, Utils.TestExecute, Utils.TestCanExecute));
            Assert.Throws<ArgumentNullException>(() => new CommandBuilderContext<object>(Utils.GetTestCommandManager(), Utils.GetTestExceptionHandler(), Utils.TestBusyStackFactory, null!, Utils.TestCanExecute));
            Assert.Throws<ArgumentNullException>(() => new CommandBuilderContext<object>(Utils.GetTestCommandManager(), Utils.GetTestExceptionHandler(), Utils.TestBusyStackFactory, Utils.TestExecute, null!));
        }

        [Fact]
        public void Ctor_ShouldNotInitializeAllProperties()
        {
            var commandManager = Utils.GetTestCommandManager();
            var context = new CommandBuilderContext<object>(commandManager, Utils.GetTestExceptionHandler(), Utils.TestBusyStackFactory, Utils.TestExecute, Utils.TestCanExecute);

            Assert.Multiple(() =>
            {
                // should not be initialized by default, since they represent optional features
                Assert.Null(context.BusyStack);
                Assert.Null(context.CancelCommand);
            });
        }

        [Fact]
        public void Ctor_ShouldInitializeDependencyProperties()
        {
            var commandManager = Utils.GetTestCommandManager();
            var context = new CommandBuilderContext<object>(commandManager, Utils.GetTestExceptionHandler(), Utils.TestBusyStackFactory, Utils.TestExecute, Utils.TestCanExecute);

            Assert.NotNull(context.CommandManager);
            Assert.Equal(commandManager, context.CommandManager);
        }

        [Fact]
        public void Build_ShouldCreateValidCommandInstance()
        {
            var commandManager = Utils.GetTestCommandManager();
            var context = new CommandBuilderContext<object>(commandManager, Utils.GetTestExceptionHandler(), Utils.TestBusyStackFactory, Utils.TestExecute, Utils.TestCanExecute);

            var command = context.Build();
            Assert.NotNull(command);
        }

        [Fact]
        public void Build_ShouldCreateDefaultCommandInstance()
        {
            var commandManager = Utils.GetTestCommandManager();
            var context = new CommandBuilderContext<object>(commandManager, Utils.GetTestExceptionHandler(), Utils.TestBusyStackFactory, Utils.TestExecute, Utils.TestCanExecute);

            var command = context.Build();

            Assert.NotNull(command.CancelCommand);
            Assert.IsType<NoCancellationCommand>(command.CancelCommand);
            Assert.Multiple(() =>
            {
                Assert.False(command.IsBusy);
                Assert.Equal(command.Completion, Task.CompletedTask);
            });
        }

        [Fact]
        public void Build_ShouldAddConcurrentCancellationSupport()
        {
            var commandManager = Utils.GetTestCommandManager();
            var command = new CommandBuilderContext<object>(commandManager, Utils.GetTestExceptionHandler(), Utils.TestBusyStackFactory, Utils.TestExecute, Utils.TestCanExecute)
                .WithAsyncCancellation()
                .Build();

            Assert.NotNull(command.CancelCommand);
            Assert.IsType<ConcurrentCancelCommand>(command.CancelCommand);
        }

        [Fact]
        public void Build_ShouldAddCancellationSupport()
        {
            var commandManager = Utils.GetTestCommandManager();
            var command = new CommandBuilderContext<object>(commandManager, Utils.GetTestExceptionHandler(), Utils.TestBusyStackFactory, Utils.TestExecute, Utils.TestCanExecute)
                .WithCancellation()
                .Build();

            Assert.NotNull(command.CancelCommand);
            Assert.IsType<CancelCommand>(command.CancelCommand);
        }

        [Fact]
        public void Build_ShouldAddCustomCancellationSupport()
        {
            var commandManager = Utils.GetTestCommandManager();
            var customCancelCommand = NSubstitute.Substitute.For<ICancelCommand>();
            var command = new CommandBuilderContext<object>(commandManager, Utils.GetTestExceptionHandler(), Utils.TestBusyStackFactory, Utils.TestExecute, Utils.TestCanExecute)
                .WithCustomCancellation(customCancelCommand)
                .Build();

            Assert.NotNull(command.CancelCommand);
            Assert.IsType<ICancelCommand>(command.CancelCommand, exactMatch: false);
        }

        [Fact]
        public void Build_ShouldThrowForNullCancelCommand()
        {
            var commandManager = Utils.GetTestCommandManager();
            var context = new CommandBuilderContext<object>(commandManager, Utils.GetTestExceptionHandler(), Utils.TestBusyStackFactory, Utils.TestExecute, Utils.TestCanExecute);

            Assert.Throws<ArgumentNullException>(() => context.WithCustomCancellation(null!));
        }

        [Fact]
        public void Build_ShouldCallAllDecorators()
        {
            var commandManager = Utils.GetTestCommandManager();
            var context = new CommandBuilderContext<object>(commandManager, Utils.GetTestExceptionHandler(), Utils.TestBusyStackFactory, Utils.TestExecute, Utils.TestCanExecute);

            var count = 0;
            const int decoratorCount = 20;

            for (var i = 0; i < decoratorCount; i++)
                context.AddDecorator(DecoratorFactory);

            var command = context.Build();

            Assert.Equal(decoratorCount, count);

            ConcurrentCommandBase DecoratorFactory(ConcurrentCommandBase command)
            {
                count++;
                return command;
            }
        }

        [Fact]
        public void Build_ShouldAddAllDecoratorsInCorrectOrder()
        {
            var commandManager = Utils.GetTestCommandManager();
            var context = new CommandBuilderContext<object>(commandManager, Utils.GetTestExceptionHandler(), Utils.TestBusyStackFactory, Utils.TestExecute, Utils.TestCanExecute);

            var count = 0;
            const int decoratorCount = 3;

            context.AddDecorator(DecoratorFactory1);
            context.AddDecorator(DecoratorFactory2);
            context.AddDecorator(DecoratorFactory3);

            var command = context.Build();

            Assert.Equal(decoratorCount, count);

            ConcurrentCommandBase DecoratorFactory1(ConcurrentCommandBase command)
            {
                Assert.Equal(0, count);
                count++;
                return command;
            }

            ConcurrentCommandBase DecoratorFactory2(ConcurrentCommandBase command)
            {
                Assert.Equal(1, count);
                count++;
                return command;
            }

            ConcurrentCommandBase DecoratorFactory3(ConcurrentCommandBase command)
            {
                Assert.Equal(2, count);
                count++;
                return command;
            }
        }

        [Fact]
        public void Build_ShouldAssignBusyStack()
        {
            var commandManager = Utils.GetTestCommandManager();
            var customBusyStack = NSubstitute.Substitute.For<IBusyStack>();
            var context = new CommandBuilderContext<object>(commandManager, Utils.GetTestExceptionHandler(), Utils.TestBusyStackFactory, Utils.TestExecute, Utils.TestCanExecute);

            context.WithBusyNotification(customBusyStack);

            Assert.IsType<IBusyStack>(context.BusyStack, exactMatch: false);
        }

        [Fact]
        public void Build_ShouldThrowForNullBusyStack()
        {
            var commandManager = Utils.GetTestCommandManager();
            var context = new CommandBuilderContext<object>(commandManager, Utils.GetTestExceptionHandler(), Utils.TestBusyStackFactory, Utils.TestExecute, Utils.TestCanExecute);

            Assert.Throws<ArgumentNullException>(() => context.WithBusyNotification(null!));
        }

        [Fact]
        public void Build_ShouldAddSingleExecutionDecorator()
        {
            var commandManager = Utils.GetTestCommandManager();
            var command = new CommandBuilderContext<object>(commandManager, Utils.GetTestExceptionHandler(), Utils.TestBusyStackFactory, Utils.TestExecute, Utils.TestCanExecute)
                .WithSingleExecution()
                .Build();

            Assert.IsType<SequentialAsyncCommandDecorator>(command);
        }

        [Fact]
        public void ExtensionsMethods_ShouldThrowOnNullInstance()
        {
            var context = default(CommandBuilderContext<object>)!;

            Assert.Throws<ArgumentNullException>(() => context.WithCancellation());
            Assert.Throws<ArgumentNullException>(() => context.WithAsyncCancellation());
            Assert.Throws<ArgumentNullException>(() => context.WithCustomCancellation(Substitute.For<ICancelCommand>()));

            Assert.Throws<ArgumentNullException>(() => context.WithBusyNotification(NSubstitute.Substitute.For<IBusyStack>()));
            Assert.Throws<ArgumentNullException>(() => context.WithSingleExecution());
        }
    }
}
