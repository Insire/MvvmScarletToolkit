using MvvmScarletToolkit.Observables;
using MvvmScarletToolkit.Tests.Util;
using NUnit.Framework;
using System;
using System.ComponentModel;

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
        public void InpcEvents_AreCached()
        {
            var eventArgs = default(PropertyChangedEventArgs);
            var vm = new DerivedViewModelBase(Utils.GetTestCommandBuilder());
            var eventArgsCount = 0;
            Assert.AreEqual(false, vm.IsBusy);

            vm.PropertyChanged += OnPropertyChangedFirst;

            vm.SetIsBusy(true);
            vm.SetIsBusy(false);

            Assert.AreEqual(2, eventArgsCount);

            void OnPropertyChangedFirst(object sender, PropertyChangedEventArgs e)
            {
                eventArgsCount++;
                eventArgs = e;

                vm.PropertyChanged -= OnPropertyChangedFirst;
                vm.PropertyChanged += OnPropertyChangedSecond;
            }

            void OnPropertyChangedSecond(object sender, PropertyChangedEventArgs e)
            {
                eventArgsCount++;
                Assert.AreEqual(eventArgs, e);
            }
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
            var commandBuilder = new ScarletCommandBuilder(dispatcher, Utils.GetTestCommandManager(), Utils.GetTestMessenger(), Utils.GetTestExitService(), Utils.GetTestEventManager(), (lambda) => new BusyStack(lambda, dispatcher));

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
