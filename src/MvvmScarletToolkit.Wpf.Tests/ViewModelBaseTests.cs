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
            Assert.Throws<ArgumentNullException>(() => new DerivedViewModelBase(null!));
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
            var commandBuilder = new ScarletCommandBuilder(dispatcher, Utils.GetTestCommandManager(), Utils.GetTestExceptionHandler(), Utils.GetTestMessenger(), Utils.GetTestExitService(), Utils.GetTestEventManager(), (lambda) => new BusyStack(lambda));

            var vm = new DerivedViewModelBase(commandBuilder);

            Assert.That(vm.IsBusy, Is.EqualTo(false));
            vm.ValidateState(() => Assert.That(vm.IsBusy, Is.EqualTo(true)));
            Assert.That(vm.IsBusy, Is.EqualTo(false));
        }
    }
}
