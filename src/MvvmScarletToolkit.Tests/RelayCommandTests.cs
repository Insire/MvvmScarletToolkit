using MvvmScarletToolkit.Commands;
using NUnit.Framework;
using System;

namespace MvvmScarletToolkit.Tests
{
    public sealed class RelayCommandTests
    {
        [Test]
        public void Ctor_DoesNotThrow()
        {
            new RelayCommand(Utils.GetTestCommandBuilder(), () => { });
        }

        [Test]
        public void Ctor_DoesThrowForNullCommandBuilder()
        {
            Assert.Throws<ArgumentNullException>(() => new RelayCommand(default(ScarletCommandBuilder), () => { }));
        }

        [Test]
        public void Ctor_DoesThrowForNullExecute()
        {
            Assert.Throws<ArgumentNullException>(() => new RelayCommand(Utils.GetTestCommandBuilder(), null));
        }

        [Test]
        public void Ctor_DoesThrowForNullCanExecute()
        {
            Assert.Throws<ArgumentNullException>(() => new RelayCommand(Utils.GetTestCommandBuilder(), () => { }, null));
        }

        [Test]
        public void Execute_IsCalledOnCommandExecution()
        {
            var executed = false;
            var command = new RelayCommand(Utils.GetTestCommandBuilder(), () => { executed = true; });
            command.Execute(null);

            Assert.IsTrue(executed);
        }

        [Test]
        public void CanExecute_IsCalledOnCommandCanExecuteCheck()
        {
            var executed = false;
            var command = new RelayCommand(Utils.GetTestCommandBuilder(), () => { }, () => { executed = true; return true; });
            command.CanExecute(null);

            Assert.IsTrue(executed);
        }
    }
}
