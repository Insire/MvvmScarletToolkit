using MvvmScarletToolkit.Commands;
using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Tests
{
    public sealed class ConcurrentCommandTests
    {
        private static ScarletCommandBuilder _builder;

        [OneTimeSetUp]
        public static void Setup()
        {
            _builder = new ScarletCommandBuilder(Utils.GetTestDispatcher(), Utils.GetTestCommandManager(), Utils.GetTestMessenger(), Utils.GetTestExitService(), Utils.GetTestEventManager(), Utils.TestBusyStackFactory);
        }

        [Test]
        public void Ctor_DoesNotThrow()
        {
            new ConcurrentCommand<object>(Utils.GetTestCommandManager(), Utils.GetTestCancelCommand(), Utils.GetTestBusyStackFactory(), (object o, CancellationToken token) => Task.CompletedTask);
        }

        [Test]
        public void Ctor_DoesThrowForNullCommandManager()
        {
            Assert.Throws<ArgumentNullException>(() => new ConcurrentCommand<object>(null, Utils.GetTestCancelCommand(), Utils.GetTestBusyStackFactory(), (object o, CancellationToken token) => Task.CompletedTask));
        }

        [Test]
        public void Ctor_DoesThrowForNullCancelCommand()
        {
            Assert.Throws<ArgumentNullException>(() => new ConcurrentCommand<object>(Utils.GetTestCommandManager(), null, Utils.GetTestBusyStackFactory(), (object o, CancellationToken token) => Task.CompletedTask));
        }

        [Test]
        public void Ctor_DoesThrowForNullBusyStackFactory()
        {
            Assert.Throws<ArgumentNullException>(() => new ConcurrentCommand<object>(Utils.GetTestCommandManager(), Utils.GetTestCancelCommand(), null, (object o, CancellationToken token) => Task.CompletedTask));
        }

        [Test]
        public void Ctor_DoesThrowForNullBusyStack()
        {
            Assert.Throws<ArgumentNullException>(() => new ConcurrentCommand<object>(Utils.GetTestCommandManager(), Utils.GetTestCancelCommand(), Utils.GetTestBusyStackFactory(), default(IBusyStack), (object o, CancellationToken token) => Task.CompletedTask, (object o) => true));
        }

        [Test]
        public void Ctor_DoesThrowForNullExecute()
        {
            Assert.Throws<ArgumentNullException>(() => new ConcurrentCommand<object>(Utils.GetTestCommandManager(), Utils.GetTestCancelCommand(), Utils.GetTestBusyStackFactory(), null));
        }

        [Test]
        public void Ctor_DoesThrowForNullCanExecute()
        {
            Assert.Throws<ArgumentNullException>(() => new ConcurrentCommand<object>(Utils.GetTestCommandManager(), Utils.GetTestCancelCommand(), Utils.GetTestBusyStackFactory(), (object o, CancellationToken token) => Task.CompletedTask, null));
        }

        [Test]
        public void Execute_IsCalledOnCommandExecution()
        {
            var executed = false;
            var command = _builder.Create((token) => { executed = true; return Task.CompletedTask; })
                .Build();

            command.Execute(null);

            Assert.IsTrue(executed);
        }

        [Test]
        public void CanExecute_IsCalledOnCommandCanExecuteCheck()
        {
            var executed = false;
            var command = _builder.Create((object o) => { Assert.Fail(); return Task.CompletedTask; }, (o) => { executed = true; return true; })
                .Build();

            command.CanExecute(default(object));

            Assert.IsTrue(executed);
        }

        [Test]
        public void Execute_IsCalledOnCommandExecutionForValueType()
        {
            var executed = false;
            var command = _builder.Create((int o) => { executed = true; return Task.CompletedTask; })
                .Build();

            command.Execute(1);

            Assert.IsTrue(executed);
        }

        [Test]
        public void Execute_IsStillCalledOnCommandExecutionForValueTypeAndNullArgument()
        {
            var executed = false;
            var command = _builder.Create((int o) => { executed = true; return Task.CompletedTask; })
                .Build();

            command.Execute(null);

            Assert.IsTrue(executed);
        }

        [Test]
        public void CanExecute_IsCalledOnCommandCanExecuteCheckForValueTypeAndNullArgument()
        {
            var executed = false;
            var command = _builder.Create((int o) => { executed = true; return Task.CompletedTask; })
                .Build();

            command.CanExecute(null);

            Assert.IsFalse(executed);
        }
    }
}
