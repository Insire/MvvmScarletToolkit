using System;

namespace MvvmScarletToolkit.Observables.Tests
{
    public sealed class NotifyProperyChangedTrackerTests
    {
        [Fact]
        public void Ctor_DoesNotThrow()
        {
            new NotifyProperyChangedTracker();
        }

        [Fact]
        public void Dispose_DoesNotThrow()
        {
            new NotifyProperyChangedTracker().Dispose();
        }

        [Fact]
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

        [Fact]
        public void NullChecks()
        {
            var viewModel = default(ViewModel)!;
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

        [Fact]
        public void Track_DoesNotThrow()
        {
            using (var tracker = new NotifyProperyChangedTracker())
            {
                var viewModel = new ViewModel();
                tracker.Track(viewModel);
            }
        }

        [Fact]
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

                viewModel.Data = string.Empty;

                Assert.Multiple(() =>
                {
                    Assert.True(propertyChanging);
                    Assert.True(propertyChanged);
                    Assert.True(tracker.HasChanges());
                    Assert.True(tracker.HasChanges(viewModel));
                    Assert.Equal(1, tracker.CountChanges(viewModel));
                });
            }

            void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
            {
                propertyChanged = true;
            }

            void ViewModel_PropertyChanging(object? sender, System.ComponentModel.PropertyChangingEventArgs e)
            {
                propertyChanging = true;
            }
        }

        [Fact]
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

                viewModel.Data = string.Empty;

                Assert.Multiple(() =>
                {
                    Assert.True(propertyChanging);
                    Assert.True(propertyChanged);
                });
                Assert.Multiple(() =>
                {
                    Assert.False(tracker.HasChanges());
                    Assert.False(tracker.HasChanges(viewModel));
                    Assert.Equal(0, tracker.CountChanges(viewModel));
                });
            }

            void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
            {
                propertyChanged = true;
            }

            void ViewModel_PropertyChanging(object? sender, System.ComponentModel.PropertyChangingEventArgs e)
            {
                propertyChanging = true;
            }
        }

        [Fact]
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

                viewModel.Data = string.Empty;

                Assert.Multiple(() =>
                {
                    Assert.True(propertyChanging);
                    Assert.True(propertyChanged);
                });
                Assert.Multiple(() =>
                {
                    Assert.False(tracker.HasChanges());
                    Assert.False(tracker.HasChanges(viewModel));
                    Assert.Equal(0, tracker.CountChanges(viewModel));
                });
            }

            void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
            {
                propertyChanged = true;
            }

            void ViewModel_PropertyChanging(object? sender, System.ComponentModel.PropertyChangingEventArgs e)
            {
                propertyChanging = true;
            }
        }

        [Fact]
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

                viewModel.Data = string.Empty;

                Assert.Multiple(() =>
                {
                    Assert.True(propertyChanging);
                    Assert.True(propertyChanged);
                    Assert.True(tracker.HasChanges());
                    Assert.True(tracker.HasChanges(viewModel));
                    Assert.Equal(1, tracker.CountChanges(viewModel));
                });

                tracker.ClearChanges(viewModel);
                Assert.Equal(0, tracker.CountChanges(viewModel));
            }

            void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
            {
                propertyChanged = true;
            }

            void ViewModel_PropertyChanging(object? sender, System.ComponentModel.PropertyChangingEventArgs e)
            {
                propertyChanging = true;
            }
        }

        [Fact]
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

                viewModel.Data = string.Empty;

                Assert.Multiple(() =>
                {
                    Assert.True(propertyChanging);
                    Assert.True(propertyChanged);
                    Assert.True(tracker.HasChanges());
                    Assert.True(tracker.HasChanges(viewModel));
                    Assert.Equal(1, tracker.CountChanges(viewModel));
                });

                tracker.ClearAllChanges();
                Assert.Equal(0, tracker.CountChanges(viewModel));
            }

            void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
            {
                propertyChanged = true;
            }

            void ViewModel_PropertyChanging(object? sender, System.ComponentModel.PropertyChangingEventArgs e)
            {
                propertyChanging = true;
            }
        }

        [Fact]
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
                    viewModel.Data = string.Empty;
                    viewModel2.Data = string.Empty;
                }

                Assert.Multiple(() =>
                {
                    Assert.True(propertyChanging);
                    Assert.True(propertyChanged);
                    Assert.True(tracker.HasChanges());
                });

                Assert.False(tracker.HasChanges(viewModel));
                Assert.Multiple(() =>
                {
                    Assert.Equal(0, tracker.CountChanges(viewModel));

                    Assert.True(tracker.HasChanges(viewModel2));
                    Assert.Equal(1, tracker.CountChanges(viewModel2));
                });
            }

            void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
            {
                propertyChanged = true;
            }

            void ViewModel_PropertyChanging(object? sender, System.ComponentModel.PropertyChangingEventArgs e)
            {
                propertyChanging = true;
            }
        }

        [Fact]
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
                    viewModel.Data = string.Empty;
                    viewModel2.Data = string.Empty;
                }

                Assert.Multiple(() =>
                {
                    Assert.True(propertyChanging);
                    Assert.True(propertyChanged);
                });
                Assert.Multiple(() =>
                {
                    Assert.False(tracker.HasChanges());

                    Assert.False(tracker.HasChanges(viewModel));
                    Assert.Equal(0, tracker.CountChanges(viewModel));

                    Assert.False(tracker.HasChanges(viewModel2));
                    Assert.Equal(0, tracker.CountChanges(viewModel2));
                });
            }

            void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
            {
                propertyChanged = true;
            }

            void ViewModel_PropertyChanging(object? sender, System.ComponentModel.PropertyChangingEventArgs e)
            {
                propertyChanging = true;
            }
        }

        [Fact]
        public void SuppressChanges_Stacked_DoesWork()
        {
            var propertyChanged = false;

            using (var tracker = new NotifyProperyChangedTracker())
            {
                var viewModel = new ViewModel();
                viewModel.PropertyChanged += ViewModel_PropertyChanged;
                tracker.Track(viewModel);

                using (tracker.SuppressAllChanges())
                {
                    var scope = tracker.SuppressChanges(viewModel);
                    scope.Dispose();

                    viewModel.Data = string.Empty;
                }

                Assert.Multiple(() =>
                {
                    Assert.True(propertyChanged);
                    Assert.False(tracker.HasChanges());

                    Assert.False(tracker.HasChanges(viewModel));
                    Assert.Equal(0, tracker.CountChanges(viewModel));
                });
            }

            void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
            {
                propertyChanged = true;
            }
        }
    }
}
