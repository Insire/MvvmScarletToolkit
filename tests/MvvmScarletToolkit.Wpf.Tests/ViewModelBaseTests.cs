using MvvmScarletToolkit.Observables;
using MvvmScarletToolkit.Tests.Util;
using System;

namespace MvvmScarletToolkit.Tests
{
    [TraceTest]
    public sealed class ViewModelBaseTests
    {
        [Fact]
        public void Ctor_DoesNotAcceptNullArgument()
        {
            Assert.Throws<ArgumentNullException>(() => new DerivedViewModelBase(null!));
        }

        [Fact]
        public void Ctor_DoesNotThrow()
        {
            new DerivedViewModelBase(Utils.GetTestCommandBuilder());
        }

        [Fact]
        public void Ctor_DoesNotThrowForNullModel()
        {
            new DerivedObjectViewModelBase(Utils.GetTestCommandBuilder(), null);
        }

        [Fact]
        public void ShouldBeBusyWhenUsingBusyStack()
        {
            var dispatcher = new TestDispatcher();
            var commandBuilder = new ScarletCommandBuilder(dispatcher, Utils.GetTestCommandManager(), Utils.GetTestExceptionHandler(), Utils.GetTestMessenger(), Utils.GetTestExitService(), Utils.GetTestEventManager(), (lambda) => new BusyStack(lambda));

            var vm = new DerivedViewModelBase(commandBuilder);

            Assert.False(vm.IsBusy);
            vm.ValidateState(() => Assert.True(vm.IsBusy));
            Assert.False(vm.IsBusy);
        }
    }
}
