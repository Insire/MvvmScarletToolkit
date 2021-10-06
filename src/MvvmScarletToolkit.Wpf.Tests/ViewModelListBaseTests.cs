using MvvmScarletToolkit.Observables;
using MvvmScarletToolkit.Tests.Util;
using NUnit.Framework;
using System;
using Microsoft.Toolkit.Mvvm.Messaging;

namespace MvvmScarletToolkit.Tests
{
    internal sealed class ViewModelListBaseTests
    {
        [Test]
        public void Ctor_DoesNotAcceptNullArgument()
        {
            Assert.Throws<ArgumentNullException>(() => new DerivedViewModelListBase(null));
        }

        [Test]
        public void Ctor_DoesNotThrow()
        {
            new DerivedViewModelListBase(Utils.GetTestCommandBuilder());
        }

        [Test]
        public void Ctor_DoesNotThrowForNullModel()
        {
            new DerivedViewModelListBase(Utils.GetTestCommandBuilder());
        }

        [Test]
        public void ShouldBeBusyWhenUsingBusyStack()
        {
            var dispatcher = new TestDispatcher();
            var commandBuilder = new ScarletCommandBuilder(dispatcher, Utils.GetTestCommandManager(), Utils.GetTestExceptionHandler(), Utils.GetTestMessenger(), Utils.GetTestExitService(), Utils.GetTestEventManager(), (lambda) => new BusyStack(lambda));

            var vm = new DerivedViewModelListBase(commandBuilder);

            Assert.AreEqual(false, vm.IsBusy);
            vm.ValidateState(() =>
            {
                Assert.AreEqual(true, vm.IsBusy);
            });
            Assert.AreEqual(false, vm.IsBusy);
        }

        [Test]
        public void ShouldSendSelectionChangedMessage()
        {
            var messenger = new WeakReferenceMessenger();

            var commandBuilder = Utils.GetTestCommandBuilder(messenger: messenger);

            var vm = new DerivedViewModelListBase(commandBuilder);
            var child1 = new DerivedObjectViewModelBase(commandBuilder, null);
            var child2 = new DerivedObjectViewModelBase(commandBuilder, null);
            vm.Add(child1);
            vm.Add(child2);

            messenger.Register<ViewModelListBaseSelectionsChanged<DerivedObjectViewModelBase>>(this, (r, m) =>
            {
                Assert.Fail();
            });

            var viewModelListBaseSelectionChangedCalled = false;
            messenger.Register<ViewModelListBaseSelectionChanged<DerivedObjectViewModelBase>>(this, (r, m) =>
            {
                viewModelListBaseSelectionChangedCalled = true;
            });

            var ViewModelListBaseSelectionChangingCalled = false;
            messenger.Register<ViewModelListBaseSelectionChanging<DerivedObjectViewModelBase>>(this, (r, m) =>
            {
                ViewModelListBaseSelectionChangingCalled = true;
            });

            vm.SelectedItem = child1;

            Assert.AreEqual(true, viewModelListBaseSelectionChangedCalled);
            Assert.AreEqual(true, ViewModelListBaseSelectionChangingCalled);
        }

        [Test]
        public void ShouldSendSelectionsChangedMessage()
        {
            var messenger = new WeakReferenceMessenger();

            var commandBuilder = Utils.GetTestCommandBuilder(messenger: messenger);

            var vm = new DerivedViewModelListBase(commandBuilder);
            var child1 = new DerivedObjectViewModelBase(commandBuilder, null);
            var child2 = new DerivedObjectViewModelBase(commandBuilder, null);
            vm.Add(child1);
            vm.Add(child2);

            var ViewModelListBaseSelectionsChangingCalled = false;
            messenger.Register<ViewModelListBaseSelectionsChanged<DerivedObjectViewModelBase>>(this, (r, m) =>
            {
                ViewModelListBaseSelectionsChangingCalled = true;
            });

            messenger.Register<ViewModelListBaseSelectionChanged<DerivedObjectViewModelBase>>(this, (r, m) =>
            {
                Assert.Fail();
            });

            messenger.Register<ViewModelListBaseSelectionChanging<DerivedObjectViewModelBase>>(this, (r, m) =>
            {
                Assert.Fail();
            });

            vm.SelectedItems.Add(child1);

            Assert.AreEqual(true, ViewModelListBaseSelectionsChangingCalled);
        }
    }
}
