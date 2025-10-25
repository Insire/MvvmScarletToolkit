using MvvmScarletToolkit.Commands;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Tests
{
    [TraceTest]
    public sealed class ConcurrentCommandTests : IAsyncLifetime
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        private static ScarletCommandBuilder _builder;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

        public ValueTask InitializeAsync()
        {
            _builder = new ScarletCommandBuilder(Utils.GetTestDispatcher(), Utils.GetTestCommandManager(), Utils.GetTestExceptionHandler(), Utils.GetTestMessenger(), Utils.GetTestExitService(), Utils.GetTestEventManager(), Utils.TestBusyStackFactory);

            return ValueTask.CompletedTask;
        }

        public ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }

        [Fact]
        public void Ctor_DoesNotThrow()
        {
            new ConcurrentCommand<object>(Utils.GetTestCommandManager(), Utils.GetTestExceptionHandler(), Utils.GetTestCancelCommand(), Utils.GetTestBusyStackFactory(), (object _, CancellationToken __) => Task.CompletedTask);
        }

        [Fact]
        public void Ctor_DoesThrowForNullCommandManager()
        {
            Assert.Throws<ArgumentNullException>(() => new ConcurrentCommand<object>(null!, Utils.GetTestExceptionHandler(), Utils.GetTestCancelCommand(), Utils.GetTestBusyStackFactory(), (object _, CancellationToken __) => Task.CompletedTask));
        }

        [Fact]
        public void Ctor_DoesThrowForNullCancelCommand()
        {
            Assert.Throws<ArgumentNullException>(() => new ConcurrentCommand<object>(Utils.GetTestCommandManager(), Utils.GetTestExceptionHandler(), null!, Utils.GetTestBusyStackFactory(), (object _, CancellationToken __) => Task.CompletedTask));
        }

        [Fact]
        public void Ctor_DoesThrowForNullBusyStackFactory()
        {
            Assert.Throws<ArgumentNullException>(() => new ConcurrentCommand<object>(Utils.GetTestCommandManager(), Utils.GetTestExceptionHandler(), Utils.GetTestCancelCommand(), null!, (object _, CancellationToken __) => Task.CompletedTask));
        }

        [Fact]
        public void Ctor_DoesThrowForNullBusyStack()
        {
            Assert.Throws<ArgumentNullException>(() => new ConcurrentCommand<object>(Utils.GetTestCommandManager(), Utils.GetTestExceptionHandler(), Utils.GetTestCancelCommand(), Utils.GetTestBusyStackFactory(), default!, (object _, CancellationToken __) => Task.CompletedTask, (object _) => true));
        }

        [Fact]
        public void Ctor_DoesThrowForNullExecute()
        {
            Assert.Throws<ArgumentNullException>(() => new ConcurrentCommand<object>(Utils.GetTestCommandManager(), Utils.GetTestExceptionHandler(), Utils.GetTestCancelCommand(), Utils.GetTestBusyStackFactory(), null!));
        }

        [Fact]
        public void Ctor_DoesThrowForNullCanExecute()
        {
            Assert.Throws<ArgumentNullException>(() => new ConcurrentCommand<object>(Utils.GetTestCommandManager(), Utils.GetTestExceptionHandler(), Utils.GetTestCancelCommand(), Utils.GetTestBusyStackFactory(), (object _, CancellationToken __) => Task.CompletedTask, null!));
        }

        [Fact]
        public void Execute_IsCalledOnCommandExecution()
        {
            var executed = false;
            var command = _builder.Create((_) => { executed = true; return Task.CompletedTask; })
                .Build();

            command.Execute(default);

            Assert.True(executed);
        }

        [Fact]
        public void CanExecute_IsCalledOnCommandCanExecuteCheck()
        {
            var executed = false;
            var command = _builder.Create((object _) => { Assert.Fail(); return Task.CompletedTask; }, (_) => { executed = true; return true; })
                .Build();

            command.CanExecute(default);

            Assert.True(executed);
        }

        [Fact]
        public void Execute_IsCalledOnCommandExecutionForValueType()
        {
            var executed = false;
            var command = _builder.Create((int _) => { executed = true; return Task.CompletedTask; })
                .Build();

            command.Execute(1);

            Assert.True(executed);
        }

        [Fact]
        public void Execute_IsStillCalledOnCommandExecutionForValueTypeAndNullArgument()
        {
            var executed = false;
            var command = _builder.Create((int _) => { executed = true; return Task.CompletedTask; })
                .Build();

            command.Execute(null);

            Assert.True(executed);
        }

        [Fact]
        public void CanExecute_IsCalledOnCommandCanExecuteCheckForValueTypeAndNullArgument()
        {
            var executed = false;
            var command = _builder.Create((int _) => { executed = true; return Task.CompletedTask; })
                .Build();

            command.CanExecute(null);

            Assert.False(executed);
        }
    }
}
