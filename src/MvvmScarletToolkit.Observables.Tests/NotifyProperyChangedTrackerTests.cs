using NUnit.Framework;

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
