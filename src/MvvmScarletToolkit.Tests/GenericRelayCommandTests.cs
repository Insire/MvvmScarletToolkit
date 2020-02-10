using MvvmScarletToolkit.Commands;
using NUnit.Framework;
using System;

namespace MvvmScarletToolkit.Tests
{
    public sealed class GenericRelayCommandTests
    {
        [Test]
        public void Ctor_DoesNotThrow()
        {
            new RelayCommand<object>(Utils.GetTestCommandBuilder(), (o) => { });
        }

        [Test]
        public void Ctor_DoesThrowForNullCommandBuilder()
        {
            Assert.Throws<ArgumentNullException>(() => new RelayCommand<object>(default(ScarletCommandBuilder), (o) => { }));
        }

        [Test]
        public void Ctor_DoesThrowForNullExecute()
        {
            Assert.Throws<ArgumentNullException>(() => new RelayCommand<object>(Utils.GetTestCommandBuilder(), null));
        }

        [Test]
        public void Ctor_DoesThrowForNullCanExecute()
        {
            Assert.Throws<ArgumentNullException>(() => new RelayCommand<object>(Utils.GetTestCommandBuilder(), (o) => { }, null));
        }

        [Test]
        public void Execute_IsCalledOnCommandExecution()
        {
            var executed = false;
            var command = new RelayCommand<object>(Utils.GetTestCommandBuilder(), (o) => { executed = true; });
            command.Execute(null);

            Assert.IsTrue(executed);
        }

        [Test]
        public void CanExecute_IsCalledOnCommandCanExecuteCheck()
        {
            var executed = false;
            var command = new RelayCommand<object>(Utils.GetTestCommandBuilder(), (o) => { Assert.Fail(); }, (o) => { executed = true; return true; });
            command.CanExecute(default(object));

            Assert.IsTrue(executed);
        }

        [Test]
        public void Execute_IsCalledOnCommandExecutionForValueType()
        {
            var executed = false;
            var command = new RelayCommand<int>(Utils.GetTestCommandBuilder(), (o) => { executed = true; });
            command.Execute(1);

            Assert.IsTrue(executed);
        }

        [Test]
        public void Execute_IsStillCalledOnCommandExecutionForValueTypeAndNullArgument()
        {
            var executed = false;
            var command = new RelayCommand<int>(Utils.GetTestCommandBuilder(), (o) => { executed = true; });
            command.Execute(null);

            Assert.IsTrue(executed);
        }

        [Test]
        public void CanExecute_IsCalledOnCommandCanExecuteCheckForValueTypeAndNullArgument()
        {
            var executed = false;
            var command = new RelayCommand<int>(Utils.GetTestCommandBuilder(), (o) => { Assert.Fail(); }, (o) => { executed = true; return true; });
            command.CanExecute(null);

            Assert.IsFalse(executed);
        }
    }
}
