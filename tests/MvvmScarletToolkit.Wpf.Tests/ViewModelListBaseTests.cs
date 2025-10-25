using CommunityToolkit.Mvvm.Messaging;
using MvvmScarletToolkit.Observables;
using MvvmScarletToolkit.Tests.Util;

namespace MvvmScarletToolkit.Tests
{
    public sealed class ViewModelListBaseTests : TraceTestBase
    {
        [Fact]
        public void Ctor_DoesNotAcceptNullArgument()
        {
            Assert.Throws<ArgumentNullException>(() => new DerivedViewModelListBase(null!));
        }

        [Fact]
        public void Ctor_DoesNotThrow()
        {
            new DerivedViewModelListBase(Utils.GetTestCommandBuilder());
        }

        [Fact]
        public void Ctor_DoesNotThrowForNullModel()
        {
            new DerivedViewModelListBase(Utils.GetTestCommandBuilder());
        }

        [Fact]
        public void ShouldBeBusyWhenUsingBusyStack()
        {
            var dispatcher = new TestDispatcher();
            var commandBuilder = new ScarletCommandBuilder(dispatcher, Utils.GetTestCommandManager(), Utils.GetTestExceptionHandler(), Utils.GetTestMessenger(), Utils.GetTestExitService(), Utils.GetTestEventManager(), (lambda) => new BusyStack(lambda));

            var vm = new DerivedViewModelListBase(commandBuilder);

            Assert.False(vm.IsBusy);
            vm.ValidateState(() => Assert.True(vm.IsBusy));
            Assert.False(vm.IsBusy);
        }

        [Fact]
        public async Task ShouldSendSelectionChangedMessage()
        {
            var messenger = new WeakReferenceMessenger();

            var commandBuilder = Utils.GetTestCommandBuilder(messenger: messenger);

            var vm = new DerivedViewModelListBase(commandBuilder);
            var child1 = new DerivedObjectViewModelBase(commandBuilder, null);
            var child2 = new DerivedObjectViewModelBase(commandBuilder, null);
            await vm.Add(child1, TestContext.Current.CancellationToken);
            await vm.Add(child2, TestContext.Current.CancellationToken);

            messenger.Register<ViewModelListBaseSelectionsChanged<DerivedObjectViewModelBase>>(this, (_, __) => Assert.Fail());

            var viewModelListBaseSelectionChangedCalled = false;
            messenger.Register<ViewModelListBaseSelectionChanged<DerivedObjectViewModelBase>>(this, (_, __) => viewModelListBaseSelectionChangedCalled = true);

            var ViewModelListBaseSelectionChangingCalled = false;
            messenger.Register<ViewModelListBaseSelectionChanging<DerivedObjectViewModelBase>>(this, (_, __) => ViewModelListBaseSelectionChangingCalled = true);

            vm.SelectedItem = child1;

            Assert.Multiple(() =>
            {
                Assert.True(viewModelListBaseSelectionChangedCalled);
                Assert.True(ViewModelListBaseSelectionChangingCalled);
            });
        }

        [Fact]
        public async Task ShouldSendSelectionsChangedMessage()
        {
            var messenger = new WeakReferenceMessenger();

            var commandBuilder = Utils.GetTestCommandBuilder(messenger: messenger);

            var vm = new DerivedViewModelListBase(commandBuilder);
            var child1 = new DerivedObjectViewModelBase(commandBuilder, new object());
            var child2 = new DerivedObjectViewModelBase(commandBuilder, new object());
            await vm.Add(child1, TestContext.Current.CancellationToken);
            await vm.Add(child2, TestContext.Current.CancellationToken);

            var ViewModelListBaseSelectionsChangingCalled = false;
            messenger.Register<ViewModelListBaseSelectionsChanged<DerivedObjectViewModelBase>>(this, (_, __) => ViewModelListBaseSelectionsChangingCalled = true);

            messenger.Register<ViewModelListBaseSelectionChanged<DerivedObjectViewModelBase>>(this, (_, __) => Assert.Fail());

            messenger.Register<ViewModelListBaseSelectionChanging<DerivedObjectViewModelBase>>(this, (_, __) => Assert.Fail());

            vm.SelectedItems.Add(child1);

            Assert.True(ViewModelListBaseSelectionsChangingCalled);
        }
    }
}
