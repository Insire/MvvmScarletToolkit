using CommunityToolkit.Mvvm.Messaging;
using System.ComponentModel;
using System.Globalization;

namespace MvvmScarletToolkit.Observables.Tests
{
    public sealed class VersionViewModelTests : TraceTestBase
    {
        [Fact]
        public void CurrentOrDefault_Does_Return_Default_Without_Changes()
        {
            var sut = new VersionViewModel<string>("default");

            Assert.Multiple(() =>
            {
                Assert.Equal("default", sut.Default);
                Assert.Null(sut.Current);
                Assert.Equal("default", sut.CurrentOrDefault);
            });
        }

        [Fact]
        public void CurrentOrDefault_Does_Return_Current_After_Change()
        {
            var sut = new VersionViewModel<string>("default");
            var properties = new List<string?>();
            sut.PropertyChanged += (_, e) => properties.Add(e.PropertyName);

            sut.Current = "current";

            Assert.Multiple(() =>
            {
                Assert.Equal("current", sut.CurrentOrDefault);
                Assert.Equal([nameof(VersionViewModel<string>.Current), nameof(VersionViewModel<string>.CurrentOrDefault)], properties);
            });
        }

        [Fact]
        public void Ctor_Does_Set_Current()
        {
            var sut = new VersionViewModel<int>(1, 2);

            Assert.Equal(2, sut.CurrentOrDefault);
        }
    }

    public sealed class ViewModelContainerTests : TraceTestBase
    {
        [Fact]
        public void Value_Is_Visible_Through_The_Untyped_Base()
        {
            var sut = new ViewModelContainer<int>(42, new WeakReferenceMessenger());
            ViewModelContainer untyped = sut;

            Assert.Equal(42, untyped.Value);

            sut.Value = 7;

            Assert.Equal(7, untyped.Value);
        }

        [Fact]
        public void IsSelected_Does_Broadcast()
        {
            var messenger = new WeakReferenceMessenger();
            var recipient = new object();
            var received = false;
            messenger.Register<CommunityToolkit.Mvvm.Messaging.Messages.PropertyChangedMessage<bool>>(recipient, (_, m) => received = m.NewValue);

            var sut = new ViewModelContainer<int>(1, messenger)
            {
                IsSelected = true,
            };

            Assert.True(received);
            GC.KeepAlive(recipient);
        }
    }

    public sealed class EnumViewModelTests : TraceTestBase
    {
        public enum Sample
        {
            [Description("first value")]
            First,
            Second,
        }

        [Fact]
        public void Create_Does_Use_Description()
        {
            var sut = EnumViewModel.Create(Sample.First, new WeakReferenceMessenger());

            Assert.Multiple(() =>
            {
                Assert.Equal(Sample.First, sut.Value);
                Assert.Equal("first value", sut.DisplayName);
            });
        }

        [Fact]
        public void Create_Does_Fallback_To_Name()
        {
            var sut = EnumViewModel.Create(Sample.Second, new WeakReferenceMessenger());

            Assert.Equal(nameof(Sample.Second), sut.DisplayName);
        }
    }

    public sealed class LocalizationsViewModelTests : TraceTestBase
    {
        [Fact]
        public void Ctor_DoesThrow_For_Null()
        {
            Assert.Throws<ArgumentNullException>(() => new LocalizationsViewModel(null!));
        }

        [Fact]
        public void Translate_Does_Use_The_Provider()
        {
            var culture = CultureInfo.CurrentUICulture;
            var provider = Substitute.For<ILocalizationProvider>();
            provider.Languages.Returns(new[] { culture });
            provider.Translate("key", culture).Returns("value");

            var sut = new LocalizationsViewModel(provider);

            Assert.Equal("value", sut.Translate("key"));
        }

        [Fact]
        public void Translate_Does_Mark_Missing_Translations()
        {
            var provider = Substitute.For<ILocalizationProvider>();
            provider.Languages.Returns(new[] { CultureInfo.CurrentUICulture });
            provider.Translate(Arg.Any<string>(), Arg.Any<CultureInfo>()).Returns(string.Empty);

            var sut = new LocalizationsViewModel(provider);

            Assert.Equal("!key!", sut.Translate("key"));
        }

        [Fact]
        public void Languages_Does_Return_Empty_For_Missing_Languages()
        {
            var provider = Substitute.For<ILocalizationProvider>();
            provider.Languages.Returns(Array.Empty<CultureInfo>());

            var sut = new LocalizationsViewModel(provider);

            Assert.Multiple(() =>
            {
                Assert.Empty(sut.Languages);
                Assert.NotNull(sut.CurrentLanguage);
            });
        }
    }

    public sealed class DispatcherProgressTests : TraceTestBase
    {
        private static IScarletDispatcher CreateDispatcher()
        {
            var dispatcher = Substitute.For<IScarletDispatcher>();
            dispatcher
                .Invoke(Arg.Any<Action?>(), Arg.Any<CancellationToken>())
                .Returns(call =>
                {
                    call.Arg<Action?>()?.Invoke();
                    return Task.CompletedTask;
                });

            return dispatcher;
        }

        [Fact]
        public void Ctor_DoesThrow_For_Null()
        {
            Assert.Throws<ArgumentNullException>(() => new DispatcherProgress<int>(null!, _ => { }, TimeSpan.FromMilliseconds(1)));
            Assert.Throws<ArgumentNullException>(() => new DispatcherProgress<int>(CreateDispatcher(), null!, TimeSpan.FromMilliseconds(1)));
        }

        [Fact]
        public async Task Report_Does_Invoke_The_Callback_With_The_Latest_Value()
        {
            var tcs = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
            using var sut = new DispatcherProgress<int>(CreateDispatcher(), value => tcs.TrySetResult(value), TimeSpan.FromMilliseconds(50));

            sut.Report(1);
            sut.Report(2);
            sut.Report(3);

            var result = await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

            Assert.Equal(3, result);
        }

        [Fact]
        public void Report_DoesThrow_After_Dispose()
        {
            var sut = new DispatcherProgress<int>(CreateDispatcher(), _ => { }, TimeSpan.FromMilliseconds(1));
            sut.Dispose();

            Assert.Throws<ObjectDisposedException>(() => sut.Report(1));
        }

        [Fact]
        public async Task Dispose_Does_Drop_Pending_Values()
        {
            var calls = 0;
            var sut = new DispatcherProgress<int>(CreateDispatcher(), _ => Interlocked.Increment(ref calls), TimeSpan.FromMilliseconds(50));

            sut.Report(1);
            sut.Dispose();
            sut.Dispose();

            await Task.Delay(200, TestContext.Current.CancellationToken);

            Assert.Equal(0, calls);
        }
    }
}
