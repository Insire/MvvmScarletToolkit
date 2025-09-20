using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Xunit.Sdk;

namespace MvvmScarletToolkit.Observables.Tests
{
    [TraceTest]
    public sealed class PropertyChangedMessageTrackerTests
    {
        public sealed class XunitDataModel : IXunitSerializable
        {
            required public Func<IMessenger, ITestViewModel> Factory { get; set; }

            public void Deserialize(IXunitSerializationInfo info)
            {
                Factory = info.GetValue<Func<IMessenger, ITestViewModel>>(nameof(Factory))!;
            }

            public void Serialize(IXunitSerializationInfo info)
            {
                info.AddValue(nameof(Factory), Factory);
            }
        }

        public static IEnumerable<TheoryDataRow<XunitDataModel>> ViewModelFactories =
            [
               new XunitDataModel {Factory =  (m) => new AttributedBroadCastViewModel(m) },
               new XunitDataModel {Factory = (m) => new BroadCastViewModel(m) },
            ];

        [Fact]
        public void Ctor_DoesNotThrow()
        {
            var messenger = new WeakReferenceMessenger();
            new PropertyChangedMessageTracker(messenger);
        }

        [Fact]
        public void Dispose_DoesNotThrow()
        {
            var messenger = new WeakReferenceMessenger();
            new PropertyChangedMessageTracker(messenger).Dispose();
        }

        [Theory]
        [MemberData(nameof(ViewModelFactories))]
        public void Dispose_DoesWork(XunitDataModel data)
        {
            var messenger = new WeakReferenceMessenger();
            var instance = new PropertyChangedMessageTracker(messenger);
            instance.Dispose();

            var viewModel = data.Factory(messenger);
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

        [Fact]
        public void NullChecks()
        {
            var messenger = new WeakReferenceMessenger();
            using (var instance = new PropertyChangedMessageTracker(messenger))
            {
                var viewModel = default(AttributedBroadCastViewModel)!;
                Assert.Throws<ArgumentNullException>(() => instance.HasChanges(viewModel));
                Assert.Throws<ArgumentNullException>(() => instance.CountChanges(viewModel));
                Assert.Throws<ArgumentNullException>(() => instance.Track<AttributedBroadCastViewModel, object>(viewModel));
                Assert.Throws<ArgumentNullException>(() => instance.StopTracking(viewModel));
                Assert.Throws<ArgumentNullException>(() => instance.ClearChanges(viewModel));
                Assert.Throws<ArgumentNullException>(() => instance.SuppressChanges(viewModel));
            }
        }

        [Theory]
        [MemberData(nameof(ViewModelFactories))]
        public void Track_DoesNotThrow(XunitDataModel data)
        {
            var messenger = new WeakReferenceMessenger();
            using (var tracker = new PropertyChangedMessageTracker(messenger))
            {
                var viewModel = data.Factory(messenger);
                tracker.Track<ITestViewModel, object>(viewModel);
            }
        }

        [Theory]
        [MemberData(nameof(ViewModelFactories))]
        public void Track_DoesWork(XunitDataModel data)
        {
            var propertyChanged = false;

            var messenger = new WeakReferenceMessenger();
            using (var tracker = new PropertyChangedMessageTracker(messenger))
            {
                var viewModel = data.Factory(messenger);
                viewModel.PropertyChanged += ViewModel_PropertyChanged;
                tracker.Track<ITestViewModel, string>(viewModel);

                viewModel.Property = string.Empty;

                Assert.Multiple(() =>
                {
                    Assert.True(propertyChanged);
                    Assert.True(tracker.HasChanges());
                    Assert.True(tracker.HasChanges(viewModel));
                    Assert.Equal(1, tracker.CountChanges(viewModel));
                });
            }

            void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
            {
                propertyChanged = true;
            }
        }

        [Theory]
        [MemberData(nameof(ViewModelFactories))]
        public void Track_With_Reset_Values_DoesWork(XunitDataModel data)
        {
            var propertyChanged = false;

            var messenger = new WeakReferenceMessenger();
            using (var tracker = new PropertyChangedMessageTracker(messenger))
            {
                var viewModel = data.Factory(messenger);
                viewModel.PropertyChanged += ViewModel_PropertyChanged;
                tracker.Track<ITestViewModel, string>(viewModel);

                viewModel.Property = string.Empty;
                viewModel.Property = null;

                Assert.Multiple(() =>
                {
                    Assert.True(propertyChanged);
                    Assert.False(tracker.HasChanges());
                    Assert.False(tracker.HasChanges(viewModel));
                    Assert.Equal(0, tracker.CountChanges(viewModel));
                });
            }

            void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
            {
                propertyChanged = true;
            }
        }

        [Theory]
        [MemberData(nameof(ViewModelFactories))]
        public void StopTracking_DoesWork(XunitDataModel data)
        {
            var propertyChanged = false;

            var messenger = new WeakReferenceMessenger();
            using (var tracker = new PropertyChangedMessageTracker(messenger))
            {
                var viewModel = data.Factory(messenger);
                viewModel.PropertyChanged += ViewModel_PropertyChanged;
                tracker.Track<ITestViewModel, string>(viewModel);

                viewModel.Property = string.Empty;

                Assert.Multiple(() =>
                {
                    Assert.True(propertyChanged);
                    Assert.True(tracker.HasChanges());
                    Assert.True(tracker.HasChanges(viewModel));
                    Assert.Equal(1, tracker.CountChanges(viewModel));
                });

                propertyChanged = false;
                tracker.StopTracking(viewModel);
                tracker.ClearAllChanges();

                viewModel.Property = null;

                Assert.Multiple(() =>
                {
                    Assert.True(propertyChanged);
                    Assert.False(tracker.HasChanges());
                    Assert.False(tracker.HasChanges(viewModel));
                    Assert.Equal(0, tracker.CountChanges(viewModel));
                });
            }

            void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
            {
                propertyChanged = true;
            }
        }

        [Theory]
        [MemberData(nameof(ViewModelFactories))]
        public void StopAllTracking_DoesWork(XunitDataModel data)
        {
            var propertyChanged = false;

            var messenger = new WeakReferenceMessenger();
            using (var tracker = new PropertyChangedMessageTracker(messenger))
            {
                var viewModel = data.Factory(messenger);
                viewModel.PropertyChanged += ViewModel_PropertyChanged;
                tracker.Track<ITestViewModel, string>(viewModel);

                viewModel.Property = string.Empty;

                Assert.Multiple(() =>
                {
                    Assert.True(propertyChanged);
                    Assert.True(tracker.HasChanges());
                    Assert.True(tracker.HasChanges(viewModel));
                    Assert.Equal(1, tracker.CountChanges(viewModel));
                });

                propertyChanged = false;
                tracker.StopAllTracking();
                tracker.ClearAllChanges();

                viewModel.Property = null;

                Assert.Multiple(() =>
                {
                    Assert.True(propertyChanged);
                    Assert.False(tracker.HasChanges());
                    Assert.False(tracker.HasChanges(viewModel));
                    Assert.Equal(0, tracker.CountChanges(viewModel));
                });
            }

            void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
            {
                propertyChanged = true;
            }
        }

        [Theory]
        [MemberData(nameof(ViewModelFactories))]
        public void SuppressChanges_DoesWork(XunitDataModel data)
        {
            var propertyChanged = false;

            var messenger = new WeakReferenceMessenger();
            using (var tracker = new PropertyChangedMessageTracker(messenger))
            {
                var viewModel = data.Factory(messenger);
                viewModel.PropertyChanged += ViewModel_PropertyChanged;
                tracker.Track<ITestViewModel, string>(viewModel);

                using (tracker.SuppressChanges(viewModel))
                {
                    viewModel.Property = string.Empty;
                }

                Assert.Multiple(() =>
                {
                    Assert.True(propertyChanged);
                    Assert.False(tracker.HasChanges());
                    Assert.False(tracker.HasChanges(viewModel));
                    Assert.Equal(0, tracker.CountChanges(viewModel));
                });
            }

            void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
            {
                propertyChanged = true;
            }
        }

        [Theory]
        [MemberData(nameof(ViewModelFactories))]
        public void SuppressAllChanges_DoesWork(XunitDataModel data)
        {
            var propertyChanged = false;

            var messenger = new WeakReferenceMessenger();
            using (var tracker = new PropertyChangedMessageTracker(messenger))
            {
                var viewModel = data.Factory(messenger);
                viewModel.PropertyChanged += ViewModel_PropertyChanged;
                tracker.Track<ITestViewModel, string>(viewModel);

                using (tracker.SuppressAllChanges())
                {
                    viewModel.Property = string.Empty;
                }

                Assert.Multiple(() =>
                {
                    Assert.True(propertyChanged);
                    Assert.False(tracker.HasChanges());
                    Assert.False(tracker.HasChanges(viewModel));
                    Assert.Equal(0, tracker.CountChanges(viewModel));
                });
            }

            void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
            {
                propertyChanged = true;
            }
        }

        [Theory]
        [MemberData(nameof(ViewModelFactories))]
        public void SuppressChanges_Stacked_DoesWork(XunitDataModel data)
        {
            var propertyChanged = false;

            var messenger = new WeakReferenceMessenger();
            using (var tracker = new PropertyChangedMessageTracker(messenger))
            {
                var viewModel = data.Factory(messenger);
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
                    Assert.True(propertyChanged);
                    Assert.False(tracker.HasChanges());
                    Assert.False(tracker.HasChanges(viewModel));
                    Assert.Equal(0, tracker.CountChanges(viewModel));
                });
            }

            void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
            {
                propertyChanged = true;
            }
        }
    }
}
