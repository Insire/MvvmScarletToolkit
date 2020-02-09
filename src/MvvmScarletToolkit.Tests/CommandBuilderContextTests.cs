using MvvmScarletToolkit.Abstractions;
using MvvmScarletToolkit.Commands;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Tests
{
    public sealed class CommandBuilderContextTests
    {
        [Test]
        public void Ctor_DoesntThrow()
        {
            new CommandBuilderContext<object>(Utils.GetTestCommandManager(), Utils.TestBusyStackFactory, Utils.TestExecute, Utils.TestCanExecute);
        }

        [Test]
        public void Ctor_ThrowsWithNullDependencies()
        {
            Assert.Throws<ArgumentNullException>(() => new CommandBuilderContext<object>(null, Utils.TestBusyStackFactory, Utils.TestExecute, Utils.TestCanExecute));
            Assert.Throws<ArgumentNullException>(() => new CommandBuilderContext<object>(Utils.GetTestCommandManager(), null, Utils.TestExecute, Utils.TestCanExecute));
            Assert.Throws<ArgumentNullException>(() => new CommandBuilderContext<object>(Utils.GetTestCommandManager(), Utils.TestBusyStackFactory, null, Utils.TestCanExecute));
            Assert.Throws<ArgumentNullException>(() => new CommandBuilderContext<object>(Utils.GetTestCommandManager(), Utils.TestBusyStackFactory, Utils.TestExecute, null));
        }

        [Test]
        public void Ctor_ShouldNotInitializeAllProperties()
        {
            var commandManager = Utils.GetTestCommandManager();
            var context = new CommandBuilderContext<object>(commandManager, Utils.TestBusyStackFactory, Utils.TestExecute, Utils.TestCanExecute);

            // should not be initialized by default, since they represent optional features
            Assert.IsNull(context.BusyStack);
            Assert.IsNull(context.CancelCommand);
        }

        [Test]
        public void Ctor_ShouldInitializeDependencyProperties()
        {
            var commandManager = Utils.GetTestCommandManager();
            var context = new CommandBuilderContext<object>(commandManager, Utils.TestBusyStackFactory, Utils.TestExecute, Utils.TestCanExecute);

            Assert.IsNotNull(context.CommandManager);
            Assert.AreEqual(commandManager, context.CommandManager);
        }

        [Test]
        public void Build_ShouldCreateValidCommandInstance()
        {
            var commandManager = Utils.GetTestCommandManager();
            var context = new CommandBuilderContext<object>(commandManager, Utils.TestBusyStackFactory, Utils.TestExecute, Utils.TestCanExecute);

            var command = context.Build();
            Assert.IsNotNull(command);
        }

        [Test]
        public void Build_ShouldCreateDefaultCommandInstance()
        {
            var commandManager = Utils.GetTestCommandManager();
            var context = new CommandBuilderContext<object>(commandManager, Utils.TestBusyStackFactory, Utils.TestExecute, Utils.TestCanExecute);

            var command = context.Build();

            Assert.IsNotNull(command.CancelCommand);
            Assert.IsInstanceOf<NoCancellationCommand>(command.CancelCommand);
            Assert.AreEqual(command.IsBusy, false);
            Assert.AreEqual(command.Completion, Task.CompletedTask);
        }

        [Test]
        public void Build_ShouldAddConcurrentCancellationSupport()
        {
            var commandManager = Utils.GetTestCommandManager();
            var command = new CommandBuilderContext<object>(commandManager, Utils.TestBusyStackFactory, Utils.TestExecute, Utils.TestCanExecute)
                .WithAsyncCancellation()
                .Build();

            Assert.IsNotNull(command.CancelCommand);
            Assert.IsInstanceOf<ConcurrentCancelCommand>(command.CancelCommand);
        }

        [Test]
        public void Build_ShouldAddCancellationSupport()
        {
            var commandManager = Utils.GetTestCommandManager();
            var command = new CommandBuilderContext<object>(commandManager, Utils.TestBusyStackFactory, Utils.TestExecute, Utils.TestCanExecute)
                .WithCancellation()
                .Build();

            Assert.IsNotNull(command.CancelCommand);
            Assert.IsInstanceOf<CancelCommand>(command.CancelCommand);
        }

        [Test]
        public void Build_ShouldAddCustomCancellationSupport()
        {
            var commandManager = Utils.GetTestCommandManager();
            var customCancelCommand = NSubstitute.Substitute.For<ICancelCommand>();
            var command = new CommandBuilderContext<object>(commandManager, Utils.TestBusyStackFactory, Utils.TestExecute, Utils.TestCanExecute)
                .WithCustomCancellation(customCancelCommand)
                .Build();

            Assert.IsNotNull(command.CancelCommand);
            Assert.IsInstanceOf<ICancelCommand>(command.CancelCommand);
        }

        [Test]
        public void Build_ShouldThrowForNullCancelCommand()
        {
            var commandManager = Utils.GetTestCommandManager();
            var context = new CommandBuilderContext<object>(commandManager, Utils.TestBusyStackFactory, Utils.TestExecute, Utils.TestCanExecute);

            Assert.Throws<ArgumentNullException>(() => context.WithCustomCancellation(null));
        }

        [Test]
        public void Build_ShouldCallAllDecorators()
        {
            var commandManager = Utils.GetTestCommandManager();
            var context = new CommandBuilderContext<object>(commandManager, Utils.TestBusyStackFactory, Utils.TestExecute, Utils.TestCanExecute);

            var count = 0;
            var decoratorCount = 20;

            for (var i = 0; i < decoratorCount; i++)
                context.AddDecorator(DecoratorFactory);

            var command = context.Build();

            Assert.AreEqual(decoratorCount, count);

            ConcurrentCommandBase DecoratorFactory(ConcurrentCommandBase command)
            {
                count++;
                return command;
            }
        }

        [Test]
        public void Build_ShouldAssignBusyStack()
        {
            var commandManager = Utils.GetTestCommandManager();
            var customBusyStack = NSubstitute.Substitute.For<IBusyStack>();
            var context = new CommandBuilderContext<object>(commandManager, Utils.TestBusyStackFactory, Utils.TestExecute, Utils.TestCanExecute);

            context.WithBusyNotification(customBusyStack);

            Assert.IsInstanceOf<IBusyStack>(context.BusyStack);
        }

        [Test]
        public void Build_ShouldThrowForNullBusyStack()
        {
            var commandManager = Utils.GetTestCommandManager();
            var context = new CommandBuilderContext<object>(commandManager, Utils.TestBusyStackFactory, Utils.TestExecute, Utils.TestCanExecute);

            Assert.Throws<ArgumentNullException>(() => context.WithBusyNotification(null));
        }

        [Test]
        public void Build_ShouldAddSingleExecutionDecorator()
        {
            var commandManager = Utils.GetTestCommandManager();
            var command = new CommandBuilderContext<object>(commandManager, Utils.TestBusyStackFactory, Utils.TestExecute, Utils.TestCanExecute)
                .WithSingleExecution()
                .Build();

            Assert.IsInstanceOf<SequentialAsyncCommandDecorator>(command);
        }

        [Test]
        public void ExtensionsMethods_ShouldThrowOnNullInstance()
        {
            var context = default(CommandBuilderContext<object>);

            Assert.Throws<ArgumentNullException>(() => context.WithCancellation());
            Assert.Throws<ArgumentNullException>(() => context.WithAsyncCancellation());
            Assert.Throws<ArgumentNullException>(() => context.WithCustomCancellation(NSubstitute.Substitute.For<ICancelCommand>()));

            Assert.Throws<ArgumentNullException>(() => context.WithBusyNotification(NSubstitute.Substitute.For<IBusyStack>()));
            Assert.Throws<ArgumentNullException>(() => context.WithSingleExecution());
        }
    }
}
