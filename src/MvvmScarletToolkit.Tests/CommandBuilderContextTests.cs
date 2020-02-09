using MvvmScarletToolkit.Commands;
using NUnit.Framework;
using System;

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
    }
}
