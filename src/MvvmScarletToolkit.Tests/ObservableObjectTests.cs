using NUnit.Framework;

namespace MvvmScarletToolkit.Tests
{
    public sealed class ObservableObjectTests
    {
        [Test]
        public void SetValue_DoesNotThrow()
        {
            new DerivedObservableObject(() => { }, () => { })
            {
                AutoProperty = new object(),
                NotifyingProperty = new object()
            };
        }

        [Test]
        public void SetValue_DoesNotThrowForNullValue()
        {
            new DerivedObservableObject(() => { }, () => { })
            {
                AutoProperty = null,
                NotifyingProperty = null
            };
        }

        [Test]
        public void SetValue_DoesNotThrowForNullOnChanged()
        {
            new DerivedObservableObject(null, () => { })
            {
                AutoProperty = new object(),
                NotifyingProperty = new object()
            };
        }

        [Test]
        public void SetValue_DoesNotThrowForNullOnChanging()
        {
            new DerivedObservableObject(() => { }, null)
            {
                AutoProperty = new object(),
                NotifyingProperty = new object()
            };
        }

        [Test]
        public void SetValue_DoesNotifyForChangedProperty()
        {
            var propertyChangedCalled = false;
            var vm = new DerivedObservableObject(null, null);
            vm.PropertyChanged += Vm_PropertyChanged;

            vm.NotifyingProperty = new object();

            Assert.AreEqual(true, propertyChangedCalled);

            void Vm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
            {
                Assert.AreEqual(nameof(DerivedObservableObject.NotifyingProperty), e.PropertyName);
                propertyChangedCalled = true;
            }
        }

        [Test]
        public void SetValue_NotificationSendsFromSender()
        {
            var vm = new DerivedObservableObject(null, null);
            vm.PropertyChanged += Vm_PropertyChanged;

            vm.NotifyingProperty = new object();

            void Vm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
            {
                Assert.AreEqual(vm, sender);
            }
        }

        [Test]
        public void SetValue_DoesNotNotifyForNonNotifyingProperty()
        {
            var propertyChangedCalled = false;
            var vm = new DerivedObservableObject(null, null);
            vm.PropertyChanged += Vm_PropertyChanged;

            vm.AutoProperty = new object();

            Assert.AreEqual(false, propertyChangedCalled);

            void Vm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
            {
                propertyChangedCalled = true;
            }
        }

        [Test]
        public void SetValue_DoesCallOnChanged()
        {
            var onChangedCalled = false;
            var vm = new DerivedObservableObject(() => { onChangedCalled = true; }, null)
            {
                NotifyingProperty = new object()
            };

            Assert.AreEqual(true, onChangedCalled);
        }

        [Test]
        public void SetValue_DoesCallOnChanging()
        {
            var onChangedCalled = false;
            var vm = new DerivedObservableObject(null, () => { onChangedCalled = true; })
            {
                NotifyingProperty = new object()
            };

            Assert.AreEqual(true, onChangedCalled);
        }

        [Test]
        public void SetValue_DoesNotNotifyWhenSettingSameValueAgain()
        {
            var onChangedCalled = 0;
            var newValue = new object();
            var vm = new DerivedObservableObject(null, () => { onChangedCalled++; })
            {
                NotifyingProperty = newValue
            };

            vm.NotifyingProperty = newValue;

            Assert.AreEqual(1, onChangedCalled);
        }
    }
}
