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
            new ScarletCommandBuilder(Utils.GetTestDispatcher(), Utils.GetTestCommandManager(), Utils.GetTestExceptionHandler(), Utils.GetTestMessenger(), Utils.GetTestExitService(), Utils.GetTestEventManager(), Utils.TestBusyStackFactory);
        }

        [Test]
        public void Ctor_ThrowsWithNullDependencies()
        {
            Assert.Throws<ArgumentNullException>(() => new ScarletCommandBuilder(null!, Utils.GetTestCommandManager(), Utils.GetTestExceptionHandler(), Utils.GetTestMessenger(), Utils.GetTestExitService(), Utils.GetTestEventManager(), Utils.TestBusyStackFactory));
            Assert.Throws<ArgumentNullException>(() => new ScarletCommandBuilder(Utils.GetTestDispatcher(), null!, Utils.GetTestExceptionHandler(), Utils.GetTestMessenger(), Utils.GetTestExitService(), Utils.GetTestEventManager(), Utils.TestBusyStackFactory));
            Assert.Throws<ArgumentNullException>(() => new ScarletCommandBuilder(Utils.GetTestDispatcher(), Utils.GetTestCommandManager(), Utils.GetTestExceptionHandler(), null!, Utils.GetTestExitService(), Utils.GetTestEventManager(), Utils.TestBusyStackFactory));
            Assert.Throws<ArgumentNullException>(() => new ScarletCommandBuilder(Utils.GetTestDispatcher(), Utils.GetTestCommandManager(), Utils.GetTestExceptionHandler(), Utils.GetTestMessenger(), null!, Utils.GetTestEventManager(), Utils.TestBusyStackFactory));
            Assert.Throws<ArgumentNullException>(() => new ScarletCommandBuilder(Utils.GetTestDispatcher(), Utils.GetTestCommandManager(), Utils.GetTestExceptionHandler(), Utils.GetTestMessenger(), Utils.GetTestExitService(), null!, Utils.TestBusyStackFactory));
            Assert.Throws<ArgumentNullException>(() => new ScarletCommandBuilder(Utils.GetTestDispatcher(), Utils.GetTestCommandManager(), Utils.GetTestExceptionHandler(), Utils.GetTestMessenger(), Utils.GetTestExitService(), Utils.GetTestEventManager(), null!));
        }

        [Test]
        public void Ctor_ShouldInitializeDependencyProperties()
        {
            var dispatcher = Utils.GetTestDispatcher();
            var commandManager = Utils.GetTestCommandManager();
            var messenger = Utils.GetTestMessenger();
            var exit = Utils.GetTestExitService();
            var eventManager = Utils.GetTestEventManager();
            var exceptionHandler = Utils.GetTestExceptionHandler();

            var builder = new ScarletCommandBuilder(dispatcher, commandManager, exceptionHandler, messenger, exit, eventManager, Utils.TestBusyStackFactory);

            Assert.Multiple(() =>
            {
                Assert.That(builder.Dispatcher, Is.EqualTo(dispatcher));
                Assert.That(builder.CommandManager, Is.EqualTo(commandManager));
                Assert.That(builder.Messenger, Is.EqualTo(messenger));
                Assert.That(builder.Exit, Is.EqualTo(exit));
                Assert.That(builder.WeakEventManager, Is.EqualTo(eventManager));
            });
        }

        [Test]
        public void Create_DoesReturnValidContextInstance()
        {
            var builder = new ScarletCommandBuilder(Utils.GetTestDispatcher(), Utils.GetTestCommandManager(), Utils.GetTestExceptionHandler(), Utils.GetTestMessenger(), Utils.GetTestExitService(), Utils.GetTestEventManager(), Utils.TestBusyStackFactory);

            var context = builder.Create<object>((_, __) => Task.CompletedTask, (_) => true);
            Assert.That(context, Is.Not.Null);
        }
    }
}
