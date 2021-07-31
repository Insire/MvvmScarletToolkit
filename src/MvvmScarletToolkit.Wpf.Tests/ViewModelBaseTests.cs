using MvvmScarletToolkit.Observables;
using MvvmScarletToolkit.Tests.Util;
using NUnit.Framework;
using System;

namespace MvvmScarletToolkit.Tests
{
    internal sealed class ViewModelBaseTests
    {
        [Test]
        public void Ctor_DoesNotAcceptNullArgument()
        {
            Assert.Throws<ArgumentNullException>(() => new DerivedViewModelBase(null));
        }

        [Test]
        public void Ctor_DoesNotThrow()
        {
            new DerivedViewModelBase(Utils.GetTestCommandBuilder());
        }

        [Test]
        public void Ctor_DoesNotThrowForNullModel()
        {
            new DerivedObjectViewModelBase(Utils.GetTestCommandBuilder(), null);
        }

        [Test]
        public void ShouldBeBusyWhenUsingBusyStack()
        {
            var dispatcher = new TestDispatcher();
            var commandBuilder = new ScarletCommandBuilder(dispatcher, Utils.GetTestCommandManager(), Utils.GetTestExceptionHandler(), Utils.GetTestMessenger(), Utils.GetTestExitService(), Utils.GetTestEventManager(), (lambda) => new BusyStack(lambda, Utils.GetTestDispatcher()));

            var vm = new DerivedViewModelBase(commandBuilder);

            Assert.AreEqual(false, vm.IsBusy);
            vm.ValidateState(() =>
            {
                Assert.AreEqual(true, vm.IsBusy);
            });
            Assert.AreEqual(false, vm.IsBusy);
        }
    }
}
