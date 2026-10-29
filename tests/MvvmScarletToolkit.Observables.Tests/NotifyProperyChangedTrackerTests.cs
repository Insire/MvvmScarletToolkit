namespace MvvmScarletToolkit.Observables.Tests
{
    public sealed class NotifyProperyChangedTrackerTests : TraceTestBase
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

        [Fact]
        public void SuppressAllChanges_Nested_DoesWork()
        {
            using (var tracker = new NotifyProperyChangedTracker())
            {
                var viewModel = new ViewModel();
                tracker.Track(viewModel);

                using (tracker.SuppressAllChanges())
                {
                    using (tracker.SuppressAllChanges())
                    {
                    }

                    // the outer scope is still active
                    viewModel.Data = string.Empty;
                }

                Assert.False(tracker.HasChanges(viewModel));

                viewModel.Data = "changed";

                Assert.True(tracker.HasChanges(viewModel));
            }
        }

        [Fact]
        public void SuppressAllChanges_Disposed_Twice_DoesWork()
        {
            using (var tracker = new NotifyProperyChangedTracker())
            {
                var viewModel = new ViewModel();
                tracker.Track(viewModel);

                using (tracker.SuppressAllChanges())
                {
                    var scope = tracker.SuppressAllChanges();
                    scope.Dispose();
                    scope.Dispose();

                    viewModel.Data = string.Empty;
                }

                Assert.False(tracker.HasChanges(viewModel));
            }
        }

        [Fact]
        public void SuppressChanges_Disposed_Twice_DoesWork()
        {
            using (var tracker = new NotifyProperyChangedTracker())
            {
                var viewModel = new ViewModel();
                tracker.Track(viewModel);

                var scope = tracker.SuppressChanges(viewModel);
                scope.Dispose();
                scope.Dispose();

                viewModel.Data = string.Empty;

                Assert.True(tracker.HasChanges(viewModel));
            }
        }

        [Fact]
        public void SuppressChanges_DoesThrow_For_Untracked_Instance()
        {
            using (var tracker = new NotifyProperyChangedTracker())
            {
                Assert.Throws<ArgumentOutOfRangeException>(() => tracker.SuppressChanges(new ViewModel()));
            }
        }

        [Fact]
        public void SuppressChanges_Does_Only_Affect_The_Instance()
        {
            using (var tracker = new NotifyProperyChangedTracker())
            {
                var suppressed = new ViewModel();
                var tracked = new ViewModel();
                tracker.Track(suppressed);
                tracker.Track(tracked);

                using (tracker.SuppressChanges(suppressed))
                {
                    suppressed.Data = string.Empty;
                    tracked.Data = string.Empty;
                }

                Assert.Multiple(() =>
                {
                    Assert.False(tracker.HasChanges(suppressed));
                    Assert.True(tracker.HasChanges(tracked));
                });
            }
        }

        [Fact]
        public void StopTracking_Does_Discard_Previous_Changes()
        {
            using (var tracker = new NotifyProperyChangedTracker())
            {
                var viewModel = new ViewModel();
                tracker.Track(viewModel);
                viewModel.Data = string.Empty;

                tracker.StopTracking(viewModel);

                Assert.Multiple(() =>
                {
                    Assert.False(tracker.HasChanges());
                    Assert.False(tracker.HasChanges(viewModel));
                });
            }
        }

        [Fact]
        public void StopAllTracking_Does_Allow_Tracking_Again()
        {
            using (var tracker = new NotifyProperyChangedTracker())
            {
                var viewModel = new ViewModel();
                tracker.Track(viewModel);
                tracker.StopAllTracking();
                tracker.Track(viewModel);

                viewModel.Data = string.Empty;

                Assert.Equal(1, tracker.CountChanges(viewModel));
            }
        }

        [Fact]
        public void Track_Twice_Does_Not_Count_Changes_Twice()
        {
            using (var tracker = new NotifyProperyChangedTracker())
            {
                var viewModel = new ViewModel();
                var notifications = 0;
                tracker.Track(viewModel);
                tracker.Track(viewModel);
                viewModel.PropertyChanged += (_, _) => notifications++;

                viewModel.Data = string.Empty;
                viewModel.Data = "changed";

                Assert.Multiple(() =>
                {
                    Assert.Equal(2, notifications);
                    Assert.Equal(1, tracker.CountChanges(viewModel));
                });
            }
        }

        [Fact]
        public void Changes_Of_Untracked_Instances_Are_Ignored()
        {
            using (var tracker = new NotifyProperyChangedTracker())
            {
                var viewModel = new ViewModel();

                viewModel.Data = string.Empty;

                Assert.Multiple(() =>
                {
                    Assert.False(tracker.HasChanges());
                    Assert.False(tracker.HasChanges(viewModel));
                    Assert.Equal(0, tracker.CountChanges(viewModel));
                });

                tracker.ClearChanges(viewModel);
                tracker.StopTracking(viewModel);
            }
        }

        [Fact]
        public void Dispose_Does_Stop_Tracking()
        {
            var viewModel = new ViewModel();
            var tracker = new NotifyProperyChangedTracker();
            tracker.Track(viewModel);
            tracker.Dispose();
            tracker.Dispose();

            viewModel.Data = string.Empty;

            Assert.False(tracker.HasChanges());
        }
    }
}
