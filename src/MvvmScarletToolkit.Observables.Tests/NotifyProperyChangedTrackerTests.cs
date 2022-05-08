using NUnit.Framework;
using System;

namespace MvvmScarletToolkit.Observables.Tests
{
    public sealed class NotifyProperyChangedTrackerTests
    {
        [Test]
        public void Ctor_DoesNotThrow()
        {
            new NotifyProperyChangedTracker();
        }

        [Test]
        public void Dispose_DoesNotThrow()
        {
            new NotifyProperyChangedTracker().Dispose();
        }

        [Test]
        public void Dispose_DoesWork()
        {
            var viewModel = new ViewModel();
            var instance = new NotifyProperyChangedTracker();
            instance.Dispose();

            Assert.Throws<ObjectDisposedException>(() => instance.HasChanges(viewModel));
            Assert.Throws<ObjectDisposedException>(() => instance.CountChanges(viewModel));
            Assert.Throws<ObjectDisposedException>(() => instance.Track(viewModel));
            Assert.Throws<ObjectDisposedException>(() => instance.StopAllTracking());
            Assert.Throws<ObjectDisposedException>(() => instance.StopTracking(viewModel));
            Assert.Throws<ObjectDisposedException>(() => instance.ClearAllChanges());
            Assert.Throws<ObjectDisposedException>(() => instance.ClearChanges(viewModel));
            Assert.Throws<ObjectDisposedException>(() => instance.SuppressAllChanges());
            Assert.Throws<ObjectDisposedException>(() => instance.SuppressChanges(viewModel));
        }

        [Test]
        public void NullChecks()
        {
            var viewModel = default(ViewModel);
            using (var instance = new NotifyProperyChangedTracker())
            {
                Assert.Throws<ArgumentNullException>(() => instance.HasChanges(viewModel));
                Assert.Throws<ArgumentNullException>(() => instance.CountChanges(viewModel));
                Assert.Throws<ArgumentNullException>(() => instance.Track(viewModel));
                Assert.Throws<ArgumentNullException>(() => instance.StopTracking(viewModel));
                Assert.Throws<ArgumentNullException>(() => instance.ClearChanges(viewModel));
                Assert.Throws<ArgumentNullException>(() => instance.SuppressChanges(viewModel));
            }
        }

        [Test]
        public void Track_DoesNotThrow()
        {
            using (var tracker = new NotifyProperyChangedTracker())
            {
                var viewModel = new ViewModel();
                tracker.Track(viewModel);
            }
        }

        [Test]
        public void Track_DoesWork()
        {
            var propertyChanging = false;
            var propertyChanged = false;

            using (var tracker = new NotifyProperyChangedTracker())
            {
                var viewModel = new ViewModel();
                viewModel.PropertyChanged += ViewModel_PropertyChanged;
                viewModel.PropertyChanging += ViewModel_PropertyChanging;
                tracker.Track(viewModel);

                viewModel.Property = new object();

                Assert.IsTrue(propertyChanging);
                Assert.IsTrue(propertyChanged);
                Assert.IsTrue(tracker.HasChanges());
                Assert.IsTrue(tracker.HasChanges(viewModel));
                Assert.AreEqual(tracker.CountChanges(viewModel), 1);
            }

            void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
            {
                propertyChanged = true;
            }

            void ViewModel_PropertyChanging(object sender, System.ComponentModel.PropertyChangingEventArgs e)
            {
                propertyChanging = true;
            }
        }

        [Test]
        public void StopTrack_DoesWork()
        {
            var propertyChanging = false;
            var propertyChanged = false;

            using (var tracker = new NotifyProperyChangedTracker())
            {
                var viewModel = new ViewModel();
                viewModel.PropertyChanged += ViewModel_PropertyChanged;
                viewModel.PropertyChanging += ViewModel_PropertyChanging;
                tracker.Track(viewModel);
                tracker.StopTracking(viewModel);
                viewModel.Property = new object();

                Assert.IsTrue(propertyChanging);
                Assert.IsTrue(propertyChanged);
                Assert.IsFalse(tracker.HasChanges());
                Assert.IsFalse(tracker.HasChanges(viewModel));
                Assert.AreEqual(tracker.CountChanges(viewModel), 0);
            }

            void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
            {
                propertyChanged = true;
            }

            void ViewModel_PropertyChanging(object sender, System.ComponentModel.PropertyChangingEventArgs e)
            {
                propertyChanging = true;
            }
        }

        [Test]
        public void StopTrackAll_DoesWork()
        {
            var propertyChanging = false;
            var propertyChanged = false;

            using (var tracker = new NotifyProperyChangedTracker())
            {
                var viewModel = new ViewModel();
                viewModel.PropertyChanged += ViewModel_PropertyChanged;
                viewModel.PropertyChanging += ViewModel_PropertyChanging;
                tracker.Track(viewModel);
                tracker.StopAllTracking();
                viewModel.Property = new object();

                Assert.IsTrue(propertyChanging);
                Assert.IsTrue(propertyChanged);
                Assert.IsFalse(tracker.HasChanges());
                Assert.IsFalse(tracker.HasChanges(viewModel));
                Assert.AreEqual(tracker.CountChanges(viewModel), 0);
            }

            void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
            {
                propertyChanged = true;
            }

            void ViewModel_PropertyChanging(object sender, System.ComponentModel.PropertyChangingEventArgs e)
            {
                propertyChanging = true;
            }
        }

        [Test]
        public void ClearChanges_DoesWork()
        {
            var propertyChanging = false;
            var propertyChanged = false;

            using (var tracker = new NotifyProperyChangedTracker())
            {
                var viewModel = new ViewModel();
                viewModel.PropertyChanged += ViewModel_PropertyChanged;
                viewModel.PropertyChanging += ViewModel_PropertyChanging;
                tracker.Track(viewModel);

                viewModel.Property = new object();

                Assert.IsTrue(propertyChanging);
                Assert.IsTrue(propertyChanged);
                Assert.IsTrue(tracker.HasChanges());
                Assert.IsTrue(tracker.HasChanges(viewModel));
                Assert.AreEqual(tracker.CountChanges(viewModel), 1);

                tracker.ClearChanges(viewModel);
                Assert.AreEqual(tracker.CountChanges(viewModel), 0);
            }

            void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
            {
                propertyChanged = true;
            }

            void ViewModel_PropertyChanging(object sender, System.ComponentModel.PropertyChangingEventArgs e)
            {
                propertyChanging = true;
            }
        }

        [Test]
        public void ClearAllChanges_DoesWork()
        {
            var propertyChanging = false;
            var propertyChanged = false;

            using (var tracker = new NotifyProperyChangedTracker())
            {
                var viewModel = new ViewModel();
                viewModel.PropertyChanged += ViewModel_PropertyChanged;
                viewModel.PropertyChanging += ViewModel_PropertyChanging;
                tracker.Track(viewModel);

                viewModel.Property = new object();

                Assert.IsTrue(propertyChanging);
                Assert.IsTrue(propertyChanged);
                Assert.IsTrue(tracker.HasChanges());
                Assert.IsTrue(tracker.HasChanges(viewModel));
                Assert.AreEqual(tracker.CountChanges(viewModel), 1);

                tracker.ClearAllChanges();
                Assert.AreEqual(tracker.CountChanges(viewModel), 0);
            }

            void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
            {
                propertyChanged = true;
            }

            void ViewModel_PropertyChanging(object sender, System.ComponentModel.PropertyChangingEventArgs e)
            {
                propertyChanging = true;
            }
        }

        [Test]
        public void SuppressChanges_DoesWork()
        {
            var propertyChanging = false;
            var propertyChanged = false;

            using (var tracker = new NotifyProperyChangedTracker())
            {
                var viewModel = new ViewModel();
                var viewModel2 = new ViewModel();
                viewModel.PropertyChanged += ViewModel_PropertyChanged;
                viewModel.PropertyChanging += ViewModel_PropertyChanging;
                tracker.Track(viewModel);
                tracker.Track(viewModel2);

                using (tracker.SuppressChanges(viewModel))
                {
                    viewModel.Property = new object();
                    viewModel2.Property = new object();
                }

                Assert.IsTrue(propertyChanging);
                Assert.IsTrue(propertyChanged);
                Assert.IsTrue(tracker.HasChanges());

                Assert.IsFalse(tracker.HasChanges(viewModel));
                Assert.AreEqual(tracker.CountChanges(viewModel), 0);

                Assert.IsTrue(tracker.HasChanges(viewModel2));
                Assert.AreEqual(tracker.CountChanges(viewModel2), 1);
            }

            void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
            {
                propertyChanged = true;
            }

            void ViewModel_PropertyChanging(object sender, System.ComponentModel.PropertyChangingEventArgs e)
            {
                propertyChanging = true;
            }
        }

        [Test]
        public void SuppressAllChanges_DoesWork()
        {
            var propertyChanging = false;
            var propertyChanged = false;

            using (var tracker = new NotifyProperyChangedTracker())
            {
                var viewModel = new ViewModel();
                var viewModel2 = new ViewModel();
                viewModel.PropertyChanged += ViewModel_PropertyChanged;
                viewModel.PropertyChanging += ViewModel_PropertyChanging;
                tracker.Track(viewModel);
                tracker.Track(viewModel2);

                using (tracker.SuppressAllChanges())
                {
                    viewModel.Property = new object();
                    viewModel2.Property = new object();
                }

                Assert.IsTrue(propertyChanging);
                Assert.IsTrue(propertyChanged);
                Assert.IsFalse(tracker.HasChanges());

                Assert.IsFalse(tracker.HasChanges(viewModel));
                Assert.AreEqual(tracker.CountChanges(viewModel), 0);

                Assert.IsFalse(tracker.HasChanges(viewModel2));
                Assert.AreEqual(tracker.CountChanges(viewModel2), 0);
            }

            void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
            {
                propertyChanged = true;
            }

            void ViewModel_PropertyChanging(object sender, System.ComponentModel.PropertyChangingEventArgs e)
            {
                propertyChanging = true;
            }
        }
    }
}
