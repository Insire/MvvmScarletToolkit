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
            _builder = new ScarletCommandBuilder(Utils.GetTestDispatcher(), Utils.GetTestCommandManager(), Utils.GetTestExceptionHandler(), Utils.GetTestMessenger(), Utils.GetTestExitService(), Utils.GetTestEventManager(), Utils.TestBusyStackFactory);
        }

        [Test]
        public void Ctor_DoesNotThrow()
        {
            new ConcurrentCommand<object>(Utils.GetTestCommandManager(), Utils.GetTestExceptionHandler(), Utils.GetTestCancelCommand(), Utils.GetTestBusyStackFactory(), (object _, CancellationToken __) => Task.CompletedTask);
        }

        [Test]
        public void Ctor_DoesThrowForNullCommandManager()
        {
            Assert.Throws<ArgumentNullException>(() => new ConcurrentCommand<object>(null!, Utils.GetTestExceptionHandler(), Utils.GetTestCancelCommand(), Utils.GetTestBusyStackFactory(), (object _, CancellationToken __) => Task.CompletedTask));
        }

        [Test]
        public void Ctor_DoesThrowForNullCancelCommand()
        {
            Assert.Throws<ArgumentNullException>(() => new ConcurrentCommand<object>(Utils.GetTestCommandManager(), Utils.GetTestExceptionHandler(), null!, Utils.GetTestBusyStackFactory(), (object _, CancellationToken __) => Task.CompletedTask));
        }

        [Test]
        public void Ctor_DoesThrowForNullBusyStackFactory()
        {
            Assert.Throws<ArgumentNullException>(() => new ConcurrentCommand<object>(Utils.GetTestCommandManager(), Utils.GetTestExceptionHandler(), Utils.GetTestCancelCommand(), null!, (object _, CancellationToken __) => Task.CompletedTask));
        }

        [Test]
        public void Ctor_DoesThrowForNullBusyStack()
        {
            Assert.Throws<ArgumentNullException>(() => new ConcurrentCommand<object>(Utils.GetTestCommandManager(), Utils.GetTestExceptionHandler(), Utils.GetTestCancelCommand(), Utils.GetTestBusyStackFactory(), default!, (object _, CancellationToken __) => Task.CompletedTask, (object _) => true));
        }

        [Test]
        public void Ctor_DoesThrowForNullExecute()
        {
            Assert.Throws<ArgumentNullException>(() => new ConcurrentCommand<object>(Utils.GetTestCommandManager(), Utils.GetTestExceptionHandler(), Utils.GetTestCancelCommand(), Utils.GetTestBusyStackFactory(), null!));
        }

        [Test]
        public void Ctor_DoesThrowForNullCanExecute()
        {
            Assert.Throws<ArgumentNullException>(() => new ConcurrentCommand<object>(Utils.GetTestCommandManager(), Utils.GetTestExceptionHandler(), Utils.GetTestCancelCommand(), Utils.GetTestBusyStackFactory(), (object _, CancellationToken __) => Task.CompletedTask, null!));
        }

        [Test]
        public void Execute_IsCalledOnCommandExecution()
        {
            var executed = false;
            var command = _builder.Create((_) => { executed = true; return Task.CompletedTask; })
                .Build();

            command.Execute(default);

            Assert.That(executed, Is.True);
        }

        [Test]
        public void CanExecute_IsCalledOnCommandCanExecuteCheck()
        {
            var executed = false;
            var command = _builder.Create((object _) => { Assert.Fail(); return Task.CompletedTask; }, (_) => { executed = true; return true; })
                .Build();

            command.CanExecute(default);

            Assert.That(executed, Is.True);
        }

        [Test]
        public void Execute_IsCalledOnCommandExecutionForValueType()
        {
            var executed = false;
            var command = _builder.Create((int _) => { executed = true; return Task.CompletedTask; })
                .Build();

            command.Execute(1);

            Assert.That(executed, Is.True);
        }

        [Test]
        public void Execute_IsStillCalledOnCommandExecutionForValueTypeAndNullArgument()
        {
            var executed = false;
            var command = _builder.Create((int _) => { executed = true; return Task.CompletedTask; })
                .Build();

            command.Execute(null);

            Assert.That(executed, Is.True);
        }

        [Test]
        public void CanExecute_IsCalledOnCommandCanExecuteCheckForValueTypeAndNullArgument()
        {
            var executed = false;
            var command = _builder.Create((int _) => { executed = true; return Task.CompletedTask; })
                .Build();

            command.CanExecute(null);

            Assert.That(executed, Is.False);
        }
    }
}
