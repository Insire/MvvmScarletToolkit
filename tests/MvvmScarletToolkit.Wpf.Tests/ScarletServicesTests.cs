using MvvmScarletToolkit.Wpf.Tests.Util;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Threading;

namespace MvvmScarletToolkit.Wpf.Tests
{
    public sealed class ScarletServicesTests : TraceTestBase
    {
        private sealed class Source : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler? PropertyChanged;

            public void Raise(string name)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }

        [Fact]
        public void LocalizationProvider_Should_Return_The_Key()
        {
            var provider = new ScarletLocalizationProvider();

            Assert.Multiple(() =>
            {
                Assert.Empty(provider.Languages);
                Assert.Equal("key", provider.Translate("key", CultureInfo.InvariantCulture));
            });
        }

        [Fact]
        public Task CommandManager_Should_Raise_RequerySuggested_Until_The_Handler_Is_Removed()
        {
            return StaRunner.Run(async () =>
            {
                var manager = ScarletCommandManager.Default;
                var count = 0;

                // the CommandManager only holds handlers weakly
                EventHandler handler = (_, _) => count++;
                manager.RequerySuggested += handler;

                manager.InvalidateRequerySuggested();
                await Ui.WaitUntil(() => count > 0);

                manager.RequerySuggested -= handler;
                var countAfterRemoval = count;

                manager.InvalidateRequerySuggested();
                await Ui.Settle();

                Assert.Equal(countAfterRemoval, count);
            });
        }

        [Fact]
        public Task WeakEventManager_Should_Forward_Events_Until_The_Handler_Is_Removed()
        {
            return StaRunner.Run(() =>
            {
                var manager = ScarletWeakEventManager.Default;
                var source = new Source();
                var names = new List<string?>();
                EventHandler<PropertyChangedEventArgs> handler = (_, e) => names.Add(e.PropertyName);

                manager.AddHandler(source, nameof(INotifyPropertyChanged.PropertyChanged), handler);
                source.Raise("first");

                manager.RemoveHandler(source, nameof(INotifyPropertyChanged.PropertyChanged), handler);
                source.Raise("second");

                Assert.Equal(["first"], names);

                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task ExitService_Should_Require_An_Application()
        {
            // the exit handling itself needs a running Application, which the test process doesn't have
            return StaRunner.Run(() =>
            {
                Assert.Throws<ArgumentNullException>(() => new ScarletExitService(null!, new ScarletDispatcher(Dispatcher.CurrentDispatcher)));

                return Task.CompletedTask;
            });
        }
    }
}
