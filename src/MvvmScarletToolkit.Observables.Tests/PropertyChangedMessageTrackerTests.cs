using CommunityToolkit.Mvvm.Messaging;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace MvvmScarletToolkit.Observables.Tests
{
    internal sealed class PropertyChangedMessageTrackerTests
    {
        public static List<Func<IMessenger, ITestViewModel>> ViewModelFactories
        {
            get
            {
                return new List<Func<IMessenger, ITestViewModel>>()
                {
                    (m) => new AttributedBroadCastViewModel(m),
                    (m) => new BroadCastViewModel(m),
                };
            }
        }

        [Test]
        public void Ctor_DoesNotThrow()
        {
            var messenger = new WeakReferenceMessenger();
            new PropertyChangedMessageTracker(messenger);
        }

        [Test]
        public void Dispose_DoesNotThrow()
        {
            var messenger = new WeakReferenceMessenger();
            new PropertyChangedMessageTracker(messenger).Dispose();
        }

        [TestCaseSource(nameof(ViewModelFactories))]
        public void Dispose_DoesWork(Func<IMessenger, ITestViewModel> factory)
        {
            var messenger = new WeakReferenceMessenger();
            var instance = new PropertyChangedMessageTracker(messenger);
            instance.Dispose();

            var viewModel = factory(messenger);
            Assert.Throws<ObjectDisposedException>(() => instance.HasChanges(viewModel));
            Assert.Throws<ObjectDisposedException>(() => instance.CountChanges(viewModel));
            Assert.Throws<ObjectDisposedException>(() => instance.Track<INotifyPropertyChanged, object>(viewModel));
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
            var messenger = new WeakReferenceMessenger();
            using (var instance = new PropertyChangedMessageTracker(messenger))
            {
                var viewModel = default(AttributedBroadCastViewModel);
                Assert.Throws<ArgumentNullException>(() => instance.HasChanges(viewModel));
                Assert.Throws<ArgumentNullException>(() => instance.CountChanges(viewModel));
                Assert.Throws<ArgumentNullException>(() => instance.Track<AttributedBroadCastViewModel, object>(viewModel));
                Assert.Throws<ArgumentNullException>(() => instance.StopTracking(viewModel));
                Assert.Throws<ArgumentNullException>(() => instance.ClearChanges(viewModel));
                Assert.Throws<ArgumentNullException>(() => instance.SuppressChanges(viewModel));
            }
        }

        [Test]
        [TestCaseSource(nameof(ViewModelFactories))]
        public void Track_DoesNotThrow(Func<IMessenger, ITestViewModel> factory)
        {
            var messenger = new WeakReferenceMessenger();
            using (var tracker = new PropertyChangedMessageTracker(messenger))
            {
                var viewModel = factory(messenger);
                tracker.Track<ITestViewModel, object>(viewModel);
            }
        }

        [Test]
        [TestCaseSource(nameof(ViewModelFactories))]
        public void Track_DoesWork(Func<IMessenger, ITestViewModel> factory)
        {
            var propertyChanged = false;

            var messenger = new WeakReferenceMessenger();
            using (var tracker = new PropertyChangedMessageTracker(messenger))
            {
                var viewModel = factory(messenger);
                viewModel.PropertyChanged += ViewModel_PropertyChanged;
                tracker.Track<ITestViewModel, string>(viewModel);

                viewModel.Property = string.Empty;

                Assert.Multiple(() =>
                {
                    Assert.That(propertyChanged, Is.True);
                    Assert.That(tracker.HasChanges(), Is.True);
                    Assert.That(tracker.HasChanges(viewModel), Is.True);
                    Assert.That(tracker.CountChanges(viewModel), Is.EqualTo(1));
                });
            }

            void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                propertyChanged = true;
            }
        }

        [Test]
        [TestCaseSource(nameof(ViewModelFactories))]
        public void Track_With_Reset_Values_DoesWork(Func<IMessenger, ITestViewModel> factory)
        {
            var propertyChanged = false;

            var messenger = new WeakReferenceMessenger();
            using (var tracker = new PropertyChangedMessageTracker(messenger))
            {
                var viewModel = factory(messenger);
                viewModel.PropertyChanged += ViewModel_PropertyChanged;
                tracker.Track<ITestViewModel, string>(viewModel);

                viewModel.Property = string.Empty;
                viewModel.Property = null;

                Assert.Multiple(() =>
                {
                    Assert.That(propertyChanged, Is.True);
                    Assert.That(tracker.HasChanges(), Is.False);
                    Assert.That(tracker.HasChanges(viewModel), Is.False);
                    Assert.That(tracker.CountChanges(viewModel), Is.EqualTo(0));
                });
            }

            void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                propertyChanged = true;
            }
        }

        [Test]
        [TestCaseSource(nameof(ViewModelFactories))]
        public void StopTracking_DoesWork(Func<IMessenger, ITestViewModel> factory)
        {
            var propertyChanged = false;

            var messenger = new WeakReferenceMessenger();
            using (var tracker = new PropertyChangedMessageTracker(messenger))
            {
                var viewModel = factory(messenger);
                viewModel.PropertyChanged += ViewModel_PropertyChanged;
                tracker.Track<ITestViewModel, string>(viewModel);

                viewModel.Property = string.Empty;

                Assert.Multiple(() =>
                {
                    Assert.That(propertyChanged, Is.True);
                    Assert.That(tracker.HasChanges(), Is.True);
                    Assert.That(tracker.HasChanges(viewModel), Is.True);
                    Assert.That(tracker.CountChanges(viewModel), Is.EqualTo(1));
                });

                propertyChanged = false;
                tracker.StopTracking(viewModel);
                tracker.ClearAllChanges();

                viewModel.Property = null;

                Assert.Multiple(() =>
                {
                    Assert.That(propertyChanged, Is.True);
                    Assert.That(tracker.HasChanges(), Is.False);
                    Assert.That(tracker.HasChanges(viewModel), Is.False);
                    Assert.That(tracker.CountChanges(viewModel), Is.EqualTo(0));
                });
            }

            void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                propertyChanged = true;
            }
        }

        [Test]
        [TestCaseSource(nameof(ViewModelFactories))]
        public void StopAllTracking_DoesWork(Func<IMessenger, ITestViewModel> factory)
        {
            var propertyChanged = false;

            var messenger = new WeakReferenceMessenger();
            using (var tracker = new PropertyChangedMessageTracker(messenger))
            {
                var viewModel = factory(messenger);
                viewModel.PropertyChanged += ViewModel_PropertyChanged;
                tracker.Track<ITestViewModel, string>(viewModel);

                viewModel.Property = string.Empty;

                Assert.Multiple(() =>
                {
                    Assert.That(propertyChanged, Is.True);
                    Assert.That(tracker.HasChanges(), Is.True);
                    Assert.That(tracker.HasChanges(viewModel), Is.True);
                    Assert.That(tracker.CountChanges(viewModel), Is.EqualTo(1));
                });

                propertyChanged = false;
                tracker.StopAllTracking();
                tracker.ClearAllChanges();

                viewModel.Property = null;

                Assert.Multiple(() =>
                {
                    Assert.That(propertyChanged, Is.True);
                    Assert.That(tracker.HasChanges(), Is.False);
                    Assert.That(tracker.HasChanges(viewModel), Is.False);
                    Assert.That(tracker.CountChanges(viewModel), Is.EqualTo(0));
                });
            }

            void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                propertyChanged = true;
            }
        }

        [Test]
        [TestCaseSource(nameof(ViewModelFactories))]
        public void SuppressChanges_DoesWork(Func<IMessenger, ITestViewModel> factory)
        {
            var propertyChanged = false;

            var messenger = new WeakReferenceMessenger();
            using (var tracker = new PropertyChangedMessageTracker(messenger))
            {
                var viewModel = factory(messenger);
                viewModel.PropertyChanged += ViewModel_PropertyChanged;
                tracker.Track<ITestViewModel, string>(viewModel);

                using (tracker.SuppressChanges(viewModel))
                {
                    viewModel.Property = string.Empty;
                }

                Assert.Multiple(() =>
                {
                    Assert.That(propertyChanged, Is.True);
                    Assert.That(tracker.HasChanges(), Is.False);
                    Assert.That(tracker.HasChanges(viewModel), Is.False);
                    Assert.That(tracker.CountChanges(viewModel), Is.EqualTo(0));
                });
            }

            void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                propertyChanged = true;
            }
        }

        [Test]
        [TestCaseSource(nameof(ViewModelFactories))]
        public void SuppressAllChanges_DoesWork(Func<IMessenger, ITestViewModel> factory)
        {
            var propertyChanged = false;

            var messenger = new WeakReferenceMessenger();
            using (var tracker = new PropertyChangedMessageTracker(messenger))
            {
                var viewModel = factory(messenger);
                viewModel.PropertyChanged += ViewModel_PropertyChanged;
                tracker.Track<ITestViewModel, string>(viewModel);

                using (tracker.SuppressAllChanges())
                {
                    viewModel.Property = string.Empty;
                }

                Assert.Multiple(() =>
                {
                    Assert.That(propertyChanged, Is.True);
                    Assert.That(tracker.HasChanges(), Is.False);
                    Assert.That(tracker.HasChanges(viewModel), Is.False);
                    Assert.That(tracker.CountChanges(viewModel), Is.EqualTo(0));
                });
            }

            void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                propertyChanged = true;
            }
        }

        [Test]
        [TestCaseSource(nameof(ViewModelFactories))]
        public void SuppressChanges_Stacked_DoesWork(Func<IMessenger, ITestViewModel> factory)
        {
            var propertyChanged = false;

            var messenger = new WeakReferenceMessenger();
            using (var tracker = new PropertyChangedMessageTracker(messenger))
            {
                var viewModel = factory(messenger);
                viewModel.PropertyChanged += ViewModel_PropertyChanged;
                tracker.Track<ITestViewModel, string>(viewModel);

                using (tracker.SuppressChanges(viewModel))
                {
                    var scope = tracker.SuppressChanges(viewModel);
                    scope.Dispose();

                    viewModel.Property = string.Empty;
                }

                Assert.Multiple(() =>
                {
                    Assert.That(propertyChanged, Is.True);
                    Assert.That(tracker.HasChanges(), Is.False);
                    Assert.That(tracker.HasChanges(viewModel), Is.False);
                    Assert.That(tracker.CountChanges(viewModel), Is.EqualTo(0));
                });
            }

            void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                propertyChanged = true;
            }
        }
    }
}
