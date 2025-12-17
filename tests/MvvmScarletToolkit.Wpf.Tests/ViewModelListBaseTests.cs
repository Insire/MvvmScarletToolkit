using CommunityToolkit.Mvvm.Messaging;
using MvvmScarletToolkit.Observables;
using MvvmScarletToolkit.Wpf.Tests.TestData;
using MvvmScarletToolkit.Wpf.Tests.Util;

namespace MvvmScarletToolkit.Wpf.Tests
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
            _=new DerivedViewModelListBase(Utils.GetTestCommandBuilder());
        }

        [Fact]
        public void Ctor_DoesNotThrowForNullModel()
        {
            _= new DerivedViewModelListBase(Utils.GetTestCommandBuilder());
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

            messenger.Register<ViewModelListBaseSelectionsChanged<DerivedObjectViewModelBase>>(this, (_, _) => Assert.Fail());

            var viewModelListBaseSelectionChangedCalled = false;
            messenger.Register<ViewModelListBaseSelectionChanged<DerivedObjectViewModelBase>>(this, (_, _) => viewModelListBaseSelectionChangedCalled = true);

            var viewModelListBaseSelectionChangingCalled = false;
            messenger.Register<ViewModelListBaseSelectionChanging<DerivedObjectViewModelBase>>(this, (_, _) => viewModelListBaseSelectionChangingCalled = true);

            vm.SelectedItem = child1;

            Assert.Multiple(() =>
            {
                Assert.True(viewModelListBaseSelectionChangedCalled);
                Assert.True(viewModelListBaseSelectionChangingCalled);
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

            var viewModelListBaseSelectionsChangingCalled = false;
            messenger.Register<ViewModelListBaseSelectionsChanged<DerivedObjectViewModelBase>>(this, (_, _) => viewModelListBaseSelectionsChangingCalled = true);

            messenger.Register<ViewModelListBaseSelectionChanged<DerivedObjectViewModelBase>>(this, (_, _) => Assert.Fail());

            messenger.Register<ViewModelListBaseSelectionChanging<DerivedObjectViewModelBase>>(this, (_, _) => Assert.Fail());

            vm.SelectedItems.Add(child1);

            Assert.True(viewModelListBaseSelectionsChangingCalled);
        }
    }
}
