using System;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Tests
{
    [TraceTest]
    public sealed class ScarletCommandBuilderTests
    {
        [Fact]
        public void Ctor_DoesntThrow()
        {
            new ScarletCommandBuilder(Utils.GetTestDispatcher(), Utils.GetTestCommandManager(), Utils.GetTestExceptionHandler(), Utils.GetTestMessenger(), Utils.GetTestExitService(), Utils.GetTestEventManager(), Utils.TestBusyStackFactory);
        }

        [Fact]
        public void Ctor_ThrowsWithNullDependencies()
        {
            Assert.Throws<ArgumentNullException>(() => new ScarletCommandBuilder(null!, Utils.GetTestCommandManager(), Utils.GetTestExceptionHandler(), Utils.GetTestMessenger(), Utils.GetTestExitService(), Utils.GetTestEventManager(), Utils.TestBusyStackFactory));
            Assert.Throws<ArgumentNullException>(() => new ScarletCommandBuilder(Utils.GetTestDispatcher(), null!, Utils.GetTestExceptionHandler(), Utils.GetTestMessenger(), Utils.GetTestExitService(), Utils.GetTestEventManager(), Utils.TestBusyStackFactory));
            Assert.Throws<ArgumentNullException>(() => new ScarletCommandBuilder(Utils.GetTestDispatcher(), Utils.GetTestCommandManager(), Utils.GetTestExceptionHandler(), null!, Utils.GetTestExitService(), Utils.GetTestEventManager(), Utils.TestBusyStackFactory));
            Assert.Throws<ArgumentNullException>(() => new ScarletCommandBuilder(Utils.GetTestDispatcher(), Utils.GetTestCommandManager(), Utils.GetTestExceptionHandler(), Utils.GetTestMessenger(), null!, Utils.GetTestEventManager(), Utils.TestBusyStackFactory));
            Assert.Throws<ArgumentNullException>(() => new ScarletCommandBuilder(Utils.GetTestDispatcher(), Utils.GetTestCommandManager(), Utils.GetTestExceptionHandler(), Utils.GetTestMessenger(), Utils.GetTestExitService(), null!, Utils.TestBusyStackFactory));
            Assert.Throws<ArgumentNullException>(() => new ScarletCommandBuilder(Utils.GetTestDispatcher(), Utils.GetTestCommandManager(), Utils.GetTestExceptionHandler(), Utils.GetTestMessenger(), Utils.GetTestExitService(), Utils.GetTestEventManager(), null!));
        }

        [Fact]
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
                Assert.Equal(dispatcher, builder.Dispatcher);
                Assert.Equal(commandManager, builder.CommandManager);
                Assert.Equal(messenger, builder.Messenger);
                Assert.Equal(exit, builder.Exit);
                Assert.Equal(eventManager, builder.WeakEventManager);
            });
        }

        [Fact]
        public void Create_DoesReturnValidContextInstance()
        {
            var builder = new ScarletCommandBuilder(Utils.GetTestDispatcher(), Utils.GetTestCommandManager(), Utils.GetTestExceptionHandler(), Utils.GetTestMessenger(), Utils.GetTestExitService(), Utils.GetTestEventManager(), Utils.TestBusyStackFactory);

            var context = builder.Create<object>((_, __) => Task.CompletedTask, (_) => true);
            Assert.NotNull(context);
        }
    }
}
