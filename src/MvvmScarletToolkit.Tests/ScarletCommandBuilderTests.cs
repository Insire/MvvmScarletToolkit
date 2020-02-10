using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Tests
{
    public sealed class ScarletCommandBuilderTests
    {
        [Test]
        public void Ctor_DoesntThrow()
        {
            new ScarletCommandBuilder(Utils.GetTestDispatcher(), Utils.GetTestCommandManager(), Utils.GetTestMessenger(), Utils.GetTestExitService(), Utils.GetTestEventManager(), Utils.TestBusyStackFactory);
        }

        [Test]
        public void Ctor_ThrowsWithNullDependencies()
        {
            Assert.Throws<ArgumentNullException>(() => new ScarletCommandBuilder(null, Utils.GetTestCommandManager(), Utils.GetTestMessenger(), Utils.GetTestExitService(), Utils.GetTestEventManager(), Utils.TestBusyStackFactory));
            Assert.Throws<ArgumentNullException>(() => new ScarletCommandBuilder(Utils.GetTestDispatcher(), null, Utils.GetTestMessenger(), Utils.GetTestExitService(), Utils.GetTestEventManager(), Utils.TestBusyStackFactory));
            Assert.Throws<ArgumentNullException>(() => new ScarletCommandBuilder(Utils.GetTestDispatcher(), Utils.GetTestCommandManager(), null, Utils.GetTestExitService(), Utils.GetTestEventManager(), Utils.TestBusyStackFactory));
            Assert.Throws<ArgumentNullException>(() => new ScarletCommandBuilder(Utils.GetTestDispatcher(), Utils.GetTestCommandManager(), Utils.GetTestMessenger(), null, Utils.GetTestEventManager(), Utils.TestBusyStackFactory));
            Assert.Throws<ArgumentNullException>(() => new ScarletCommandBuilder(Utils.GetTestDispatcher(), Utils.GetTestCommandManager(), Utils.GetTestMessenger(), Utils.GetTestExitService(), null, Utils.TestBusyStackFactory));
            Assert.Throws<ArgumentNullException>(() => new ScarletCommandBuilder(Utils.GetTestDispatcher(), Utils.GetTestCommandManager(), Utils.GetTestMessenger(), Utils.GetTestExitService(), Utils.GetTestEventManager(), null));
        }

        [Test]
        public void Ctor_ShouldInitializeDependencyProperties()
        {
            var dispatcher = Utils.GetTestDispatcher();
            var commandManager = Utils.GetTestCommandManager();
            var messenger = Utils.GetTestMessenger();
            var exit = Utils.GetTestExitService();
            var eventManager = Utils.GetTestEventManager();

            var builder = new ScarletCommandBuilder(dispatcher, commandManager, messenger, exit, eventManager, Utils.TestBusyStackFactory);

            Assert.AreEqual(dispatcher, builder.Dispatcher);
            Assert.AreEqual(commandManager, builder.CommandManager);
            Assert.AreEqual(messenger, builder.Messenger);
            Assert.AreEqual(exit, builder.Exit);
            Assert.AreEqual(eventManager, builder.WeakEventManager);
        }

        [Test]
        public void Create_DoesReturnValidContextInstance()
        {
            var builder = new ScarletCommandBuilder(Utils.GetTestDispatcher(), Utils.GetTestCommandManager(), Utils.GetTestMessenger(), Utils.GetTestExitService(), Utils.GetTestEventManager(), Utils.TestBusyStackFactory);

            var context = builder.Create<object>((o, token) => Task.CompletedTask, (o) => true);
            Assert.IsNotNull(context);
        }
    }
}
