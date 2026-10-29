using CommunityToolkit.Mvvm.Messaging;
using System.ComponentModel;
using Xunit.Sdk;

namespace MvvmScarletToolkit.Observables.Tests
{
    public sealed class PropertyChangedMessageTrackerTests : TraceTestBase
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

        [Theory]
        [MemberData(nameof(ViewModelFactories))]
        public void SuppressAllChanges_Can_Be_Used_Repeatedly(XunitDataModel data)
        {
            var messenger = new WeakReferenceMessenger();
            using (var tracker = new PropertyChangedMessageTracker(messenger))
            {
                var viewModel = data.Factory(messenger);
                tracker.Track<ITestViewModel, string>(viewModel);

                using (tracker.SuppressAllChanges())
                {
                    viewModel.Property = string.Empty;
                }

                using (tracker.SuppressAllChanges())
                {
                    using (tracker.SuppressAllChanges())
                    {
                    }

                    viewModel.Property = "suppressed";
                }

                Assert.False(tracker.HasChanges(viewModel));

                viewModel.Property = "changed";

                Assert.True(tracker.HasChanges(viewModel));
            }
        }

        [Theory]
        [MemberData(nameof(ViewModelFactories))]
        public void SuppressChanges_Disposed_Twice_DoesWork(XunitDataModel data)
        {
            var messenger = new WeakReferenceMessenger();
            using (var tracker = new PropertyChangedMessageTracker(messenger))
            {
                var viewModel = data.Factory(messenger);
                tracker.Track<ITestViewModel, string>(viewModel);

                using (tracker.SuppressChanges(viewModel))
                {
                    var scope = tracker.SuppressChanges(viewModel);
                    scope.Dispose();
                    scope.Dispose();

                    viewModel.Property = string.Empty;
                }

                Assert.False(tracker.HasChanges(viewModel));

                viewModel.Property = "changed";

                Assert.True(tracker.HasChanges(viewModel));
            }
        }

        [Fact]
        public void SuppressChanges_DoesThrow_For_Untracked_Instance()
        {
            var messenger = new WeakReferenceMessenger();
            using (var tracker = new PropertyChangedMessageTracker(messenger))
            {
                Assert.Throws<InvalidOperationException>(() => tracker.SuppressChanges(new BroadCastViewModel(messenger)));
            }
        }

        [Theory]
        [MemberData(nameof(ViewModelFactories))]
        public void Track_Multiple_Instances_DoesWork(XunitDataModel data)
        {
            var messenger = new WeakReferenceMessenger();
            using (var tracker = new PropertyChangedMessageTracker(messenger))
            {
                var first = data.Factory(messenger);
                var second = data.Factory(messenger);
                tracker.Track<ITestViewModel, string>(first);
                tracker.Track<ITestViewModel, string>(second);

                second.Property = string.Empty;

                Assert.Multiple(() =>
                {
                    Assert.False(tracker.HasChanges(first));
                    Assert.True(tracker.HasChanges(second));
                    Assert.True(tracker.HasChanges());
                });
            }
        }

        [Theory]
        [MemberData(nameof(ViewModelFactories))]
        public void Track_Same_Instance_Twice_Does_Keep_Changes(XunitDataModel data)
        {
            var messenger = new WeakReferenceMessenger();
            using (var tracker = new PropertyChangedMessageTracker(messenger))
            {
                var viewModel = data.Factory(messenger);
                tracker.Track<ITestViewModel, string>(viewModel);
                viewModel.Property = string.Empty;

                tracker.Track<ITestViewModel, string>(viewModel);
                tracker.Track<ITestViewModel, int>(viewModel);

                Assert.Equal(1, tracker.CountChanges(viewModel));
            }
        }

        [Theory]
        [MemberData(nameof(ViewModelFactories))]
        public void ClearChanges_Does_Keep_Tracking(XunitDataModel data)
        {
            var messenger = new WeakReferenceMessenger();
            using (var tracker = new PropertyChangedMessageTracker(messenger))
            {
                var viewModel = data.Factory(messenger);
                tracker.Track<ITestViewModel, string>(viewModel);
                viewModel.Property = string.Empty;

                tracker.ClearChanges(viewModel);

                Assert.False(tracker.HasChanges(viewModel));

                viewModel.Property = "changed";

                Assert.True(tracker.HasChanges(viewModel));
            }
        }

        [Theory]
        [MemberData(nameof(ViewModelFactories))]
        public void ClearAllChanges_Does_Keep_Tracking(XunitDataModel data)
        {
            var messenger = new WeakReferenceMessenger();
            using (var tracker = new PropertyChangedMessageTracker(messenger))
            {
                var viewModel = data.Factory(messenger);
                tracker.Track<ITestViewModel, string>(viewModel);
                viewModel.Property = string.Empty;

                tracker.ClearAllChanges();

                Assert.False(tracker.HasChanges());

                viewModel.Property = "changed";

                Assert.True(tracker.HasChanges());
            }
        }

        [Theory]
        [MemberData(nameof(ViewModelFactories))]
        public void Track_Change_After_Revert_DoesWork(XunitDataModel data)
        {
            var messenger = new WeakReferenceMessenger();
            using (var tracker = new PropertyChangedMessageTracker(messenger))
            {
                var viewModel = data.Factory(messenger);
                tracker.Track<ITestViewModel, string>(viewModel);

                viewModel.Property = "a";
                viewModel.Property = null;
                viewModel.Property = "b";

                Assert.True(tracker.TryGetChangeFor<ITestViewModel, string>(viewModel, nameof(ITestViewModel.Property), out var change));
                Assert.Multiple(() =>
                {
                    Assert.True(tracker.HasChanges(viewModel));
                    Assert.Equal(1, tracker.CountChanges(viewModel));
                    Assert.NotNull(change);
                    Assert.True(change!.IsActualChange);
                    Assert.Null(change.InitialValue);
                    Assert.Equal("b", change.NewValue);
                });
            }
        }

        [Theory]
        [MemberData(nameof(ViewModelFactories))]
        public void TryGetChangeFor_DoesWork(XunitDataModel data)
        {
            var messenger = new WeakReferenceMessenger();
            using (var tracker = new PropertyChangedMessageTracker(messenger))
            {
                var viewModel = data.Factory(messenger);
                tracker.Track<ITestViewModel, string>(viewModel);

                Assert.False(tracker.TryGetChangeFor(viewModel, nameof(ITestViewModel.Property), out _));

                viewModel.Property = "a";

                Assert.Multiple(() =>
                {
                    Assert.True(tracker.TryGetChangeFor(viewModel, nameof(ITestViewModel.Property), out var untyped));
                    Assert.True(untyped!.IsActualChange);

                    // asking for the wrong property type must not throw
                    Assert.False(tracker.TryGetChangeFor<ITestViewModel, int>(viewModel, nameof(ITestViewModel.Property), out var wrongType));
                    Assert.Null(wrongType);

                    Assert.False(tracker.TryGetChangeFor(new object(), nameof(ITestViewModel.Property), out _));
                });
            }
        }

        [Theory]
        [MemberData(nameof(ViewModelFactories))]
        public void Dispose_Does_Unregister_From_Messenger(XunitDataModel data)
        {
            var messenger = new WeakReferenceMessenger();
            var tracker = new PropertyChangedMessageTracker(messenger);
            var viewModel = data.Factory(messenger);
            tracker.Track<ITestViewModel, string>(viewModel);

            tracker.Dispose();
            tracker.Dispose();

            Assert.False(messenger.IsRegistered<CommunityToolkit.Mvvm.Messaging.Messages.PropertyChangedMessage<string>>(tracker));
        }
    }
}
