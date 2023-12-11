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
            var _ = new CommandBuilderContext<object>(Utils.GetTestCommandManager(), Utils.GetTestExceptionHandler(), Utils.TestBusyStackFactory, Utils.TestExecute, Utils.TestCanExecute);
        }

        [Test]
        public void Ctor_ThrowsWithNullDependencies()
        {
            Assert.Throws<ArgumentNullException>(() => new CommandBuilderContext<object>(null, Utils.GetTestExceptionHandler(), Utils.TestBusyStackFactory, Utils.TestExecute, Utils.TestCanExecute));
            Assert.Throws<ArgumentNullException>(() => new CommandBuilderContext<object>(Utils.GetTestCommandManager(), Utils.GetTestExceptionHandler(), null, Utils.TestExecute, Utils.TestCanExecute));
            Assert.Throws<ArgumentNullException>(() => new CommandBuilderContext<object>(Utils.GetTestCommandManager(), Utils.GetTestExceptionHandler(), Utils.TestBusyStackFactory, null, Utils.TestCanExecute));
            Assert.Throws<ArgumentNullException>(() => new CommandBuilderContext<object>(Utils.GetTestCommandManager(), Utils.GetTestExceptionHandler(), Utils.TestBusyStackFactory, Utils.TestExecute, null));
        }

        [Test]
        public void Ctor_ShouldNotInitializeAllProperties()
        {
            var commandManager = Utils.GetTestCommandManager();
            var context = new CommandBuilderContext<object>(commandManager, Utils.GetTestExceptionHandler(), Utils.TestBusyStackFactory, Utils.TestExecute, Utils.TestCanExecute);

            Assert.Multiple(() =>
            {
                // should not be initialized by default, since they represent optional features
                Assert.That(context.BusyStack, Is.Null);
                Assert.That(context.CancelCommand, Is.Null);
            });
        }

        [Test]
        public void Ctor_ShouldInitializeDependencyProperties()
        {
            var commandManager = Utils.GetTestCommandManager();
            var context = new CommandBuilderContext<object>(commandManager, Utils.GetTestExceptionHandler(), Utils.TestBusyStackFactory, Utils.TestExecute, Utils.TestCanExecute);

            Assert.That(context.CommandManager, Is.Not.Null);
            Assert.That(context.CommandManager, Is.EqualTo(commandManager));
        }

        [Test]
        public void Build_ShouldCreateValidCommandInstance()
        {
            var commandManager = Utils.GetTestCommandManager();
            var context = new CommandBuilderContext<object>(commandManager, Utils.GetTestExceptionHandler(), Utils.TestBusyStackFactory, Utils.TestExecute, Utils.TestCanExecute);

            var command = context.Build();
            Assert.That(command, Is.Not.Null);
        }

        [Test]
        public void Build_ShouldCreateDefaultCommandInstance()
        {
            var commandManager = Utils.GetTestCommandManager();
            var context = new CommandBuilderContext<object>(commandManager, Utils.GetTestExceptionHandler(), Utils.TestBusyStackFactory, Utils.TestExecute, Utils.TestCanExecute);

            var command = context.Build();

            Assert.That(command.CancelCommand, Is.Not.Null);
            Assert.That(command.CancelCommand, Is.InstanceOf<NoCancellationCommand>());
            Assert.Multiple(() =>
            {
                Assert.That(command.IsBusy, Is.EqualTo(false));
                Assert.That(Task.CompletedTask, Is.EqualTo(command.Completion));
            });
        }

        [Test]
        public void Build_ShouldAddConcurrentCancellationSupport()
        {
            var commandManager = Utils.GetTestCommandManager();
            var command = new CommandBuilderContext<object>(commandManager, Utils.GetTestExceptionHandler(), Utils.TestBusyStackFactory, Utils.TestExecute, Utils.TestCanExecute)
                .WithAsyncCancellation()
                .Build();

            Assert.That(command.CancelCommand, Is.Not.Null);
            Assert.That(command.CancelCommand, Is.InstanceOf<ConcurrentCancelCommand>());
        }

        [Test]
        public void Build_ShouldAddCancellationSupport()
        {
            var commandManager = Utils.GetTestCommandManager();
            var command = new CommandBuilderContext<object>(commandManager, Utils.GetTestExceptionHandler(), Utils.TestBusyStackFactory, Utils.TestExecute, Utils.TestCanExecute)
                .WithCancellation()
                .Build();

            Assert.That(command.CancelCommand, Is.Not.Null);
            Assert.That(command.CancelCommand, Is.InstanceOf<CancelCommand>());
        }

        [Test]
        public void Build_ShouldAddCustomCancellationSupport()
        {
            var commandManager = Utils.GetTestCommandManager();
            var customCancelCommand = NSubstitute.Substitute.For<ICancelCommand>();
            var command = new CommandBuilderContext<object>(commandManager, Utils.GetTestExceptionHandler(), Utils.TestBusyStackFactory, Utils.TestExecute, Utils.TestCanExecute)
                .WithCustomCancellation(customCancelCommand)
                .Build();

            Assert.That(command.CancelCommand, Is.Not.Null);
            Assert.That(command.CancelCommand, Is.InstanceOf<ICancelCommand>());
        }

        [Test]
        public void Build_ShouldThrowForNullCancelCommand()
        {
            var commandManager = Utils.GetTestCommandManager();
            var context = new CommandBuilderContext<object>(commandManager, Utils.GetTestExceptionHandler(), Utils.TestBusyStackFactory, Utils.TestExecute, Utils.TestCanExecute);

            Assert.Throws<ArgumentNullException>(() => context.WithCustomCancellation(null));
        }

        [Test]
        public void Build_ShouldCallAllDecorators()
        {
            var commandManager = Utils.GetTestCommandManager();
            var context = new CommandBuilderContext<object>(commandManager, Utils.GetTestExceptionHandler(), Utils.TestBusyStackFactory, Utils.TestExecute, Utils.TestCanExecute);

            var count = 0;
            const int decoratorCount = 20;

            for (var i = 0; i < decoratorCount; i++)
                context.AddDecorator(DecoratorFactory);

            var command = context.Build();

            Assert.That(count, Is.EqualTo(decoratorCount));

            ConcurrentCommandBase DecoratorFactory(ConcurrentCommandBase command)
            {
                count++;
                return command;
            }
        }

        [Test]
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

            Assert.That(count, Is.EqualTo(decoratorCount));

            ConcurrentCommandBase DecoratorFactory1(ConcurrentCommandBase command)
            {
                Assert.That(count, Is.EqualTo(0));
                count++;
                return command;
            }

            ConcurrentCommandBase DecoratorFactory2(ConcurrentCommandBase command)
            {
                Assert.That(count, Is.EqualTo(1));
                count++;
                return command;
            }

            ConcurrentCommandBase DecoratorFactory3(ConcurrentCommandBase command)
            {
                Assert.That(count, Is.EqualTo(2));
                count++;
                return command;
            }
        }

        [Test]
        public void Build_ShouldAssignBusyStack()
        {
            var commandManager = Utils.GetTestCommandManager();
            var customBusyStack = NSubstitute.Substitute.For<IBusyStack>();
            var context = new CommandBuilderContext<object>(commandManager, Utils.GetTestExceptionHandler(), Utils.TestBusyStackFactory, Utils.TestExecute, Utils.TestCanExecute);

            context.WithBusyNotification(customBusyStack);

            Assert.That(context.BusyStack, Is.InstanceOf<IBusyStack>());
        }

        [Test]
        public void Build_ShouldThrowForNullBusyStack()
        {
            var commandManager = Utils.GetTestCommandManager();
            var context = new CommandBuilderContext<object>(commandManager, Utils.GetTestExceptionHandler(), Utils.TestBusyStackFactory, Utils.TestExecute, Utils.TestCanExecute);

            Assert.Throws<ArgumentNullException>(() => context.WithBusyNotification(null));
        }

        [Test]
        public void Build_ShouldAddSingleExecutionDecorator()
        {
            var commandManager = Utils.GetTestCommandManager();
            var command = new CommandBuilderContext<object>(commandManager, Utils.GetTestExceptionHandler(), Utils.TestBusyStackFactory, Utils.TestExecute, Utils.TestCanExecute)
                .WithSingleExecution()
                .Build();

            Assert.That(command, Is.InstanceOf<SequentialAsyncCommandDecorator>());
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
