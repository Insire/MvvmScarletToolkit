using CommunityToolkit.Mvvm.ComponentModel;
using MvvmScarletToolkit.Commands;
using MvvmScarletToolkit.Observables;
using MvvmScarletToolkit.Wpf.Tests.Util;
using System.Windows.Input;

namespace MvvmScarletToolkit.Wpf.Tests
{
    public sealed class PagingTests : TraceTestBase
    {
        public sealed class Entry(string name) : ObservableObject
        {
            public string Name { get; } = name;
        }

        private sealed class PagedList(IScarletCommandBuilder commandBuilder, IPagedDataProvider<Entry> provider)
            : PagedSourceListViewModelBase<Entry>(commandBuilder, new SynchronizationContext(), p => p.Name, provider)
        {
            public void SetTotalPageCount(int value) => TotalPageCount = value;
        }

        private sealed class DomainList(IScarletCommandBuilder commandBuilder)
            : DomainViewModelListBase<Entry>(commandBuilder, [10])
        {
            public void SetTotal(int value) => Total = value;

            protected override Task RefreshInternal(CancellationToken token) => Task.CompletedTask;
        }

        private sealed class SourceList(IScarletCommandBuilder commandBuilder)
            : BusinessSourceListViewModelBase<Entry>(commandBuilder, new SynchronizationContext(), p => p.Name)
        {
            protected override Task RefreshInternal(CancellationToken token) => Task.CompletedTask;
        }

        private static IScarletCommandBuilder CreateCommandBuilder()
        {
            return Utils.GetTestCommandBuilder(dispatcher: new TestDispatcher());
        }

        private static Task Execute(ICommand command)
        {
            return ((ConcurrentCommandBase)command).ExecuteAsync(null);
        }

        private static IPagedDataProvider<Entry> CreateProvider()
        {
            var provider = Substitute.For<IPagedDataProvider<Entry>>();
            provider
                .Get(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
                .Returns(call => Task.FromResult<ICollection<Entry>>([new Entry($"{call.ArgAt<int>(0)}")]));

            return provider;
        }

        private static async Task<PagedList> CreateLoadedPagedList(IPagedDataProvider<Entry> provider, int totalPageCount)
        {
            var sut = new PagedList(CreateCommandBuilder(), provider)
            {
                PageSize = 10,
            };
            sut.SetTotalPageCount(totalPageCount);

            await Execute(sut.LoadCommand);
            return sut;
        }

        [Fact]
        public async Task PagedSourceList_Load_Does_Request_The_First_Page()
        {
            var provider = CreateProvider();
            var sut = await CreateLoadedPagedList(provider, 3);

            Assert.Multiple(() =>
            {
                Assert.Equal(1, sut.CurrentPage);
                Assert.True(sut.IsLoaded);
                Assert.True(sut.CanUnload());
                Assert.False(sut.PreviousCommand.CanExecute(null));
                Assert.False(sut.FirstCommand.CanExecute(null));
                Assert.True(sut.NextCommand.CanExecute(null));
                Assert.True(sut.LastCommand.CanExecute(null));
            });

            await provider.Received(1).Get(0, 10, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task PagedSourceList_Next_Does_Reach_The_Last_Page()
        {
            var provider = CreateProvider();
            var sut = await CreateLoadedPagedList(provider, 3);

            await Execute(sut.NextCommand);
            await Execute(sut.NextCommand);

            Assert.Multiple(() =>
            {
                Assert.Equal(3, sut.CurrentPage);
                Assert.False(sut.NextCommand.CanExecute(null));
                Assert.False(sut.LastCommand.CanExecute(null));
                Assert.True(sut.PreviousCommand.CanExecute(null));
            });

            Received.InOrder(() =>
            {
                provider.Get(0, 10, Arg.Any<CancellationToken>());
                provider.Get(10, 10, Arg.Any<CancellationToken>());
                provider.Get(20, 10, Arg.Any<CancellationToken>());
            });
        }

        [Fact]
        public async Task PagedSourceList_Last_And_First_Do_Work()
        {
            var provider = CreateProvider();
            var sut = await CreateLoadedPagedList(provider, 5);

            await Execute(sut.LastCommand);

            Assert.Equal(5, sut.CurrentPage);
            await provider.Received(1).Get(40, 10, Arg.Any<CancellationToken>());

            await Execute(sut.FirstCommand);

            Assert.Equal(1, sut.CurrentPage);
            await provider.Received(2).Get(0, 10, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task PagedSourceList_Previous_Does_Return_To_The_First_Page()
        {
            var provider = CreateProvider();
            var sut = await CreateLoadedPagedList(provider, 3);

            await Execute(sut.NextCommand);
            await Execute(sut.PreviousCommand);

            Assert.Multiple(() =>
            {
                Assert.Equal(1, sut.CurrentPage);
                Assert.False(sut.PreviousCommand.CanExecute(null));
            });

            await provider.Received(2).Get(0, 10, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task PagedSourceList_Single_Page_Does_Not_Allow_Navigation()
        {
            var sut = await CreateLoadedPagedList(CreateProvider(), 1);

            Assert.Multiple(() =>
            {
                Assert.False(sut.NextCommand.CanExecute(null));
                Assert.False(sut.LastCommand.CanExecute(null));
                Assert.False(sut.PreviousCommand.CanExecute(null));
                Assert.False(sut.FirstCommand.CanExecute(null));
            });
        }

        [Fact]
        public async Task PagedSourceList_Unload_DoesWork()
        {
            var sut = await CreateLoadedPagedList(CreateProvider(), 1);

            await Execute(sut.UnloadCommand);

            Assert.Multiple(() =>
            {
                Assert.False(sut.IsLoaded);
                Assert.False(sut.CanUnload());
                Assert.True(sut.CanLoad());
            });
        }

        private static async Task<DomainList> CreateLoadedDomainList(int total)
        {
            var sut = new DomainList(CreateCommandBuilder())
            {
                PageSize = 10,
            };
            sut.SetTotal(total);

            await Execute(sut.LoadCommand);
            return sut;
        }

        [Fact]
        public async Task PagingViewModel_Next_Does_Reach_The_Last_Partial_Page()
        {
            // 25 entries with 10 per page -> pages 0, 1, 2
            var sut = await CreateLoadedDomainList(25);

            Assert.Multiple(() =>
            {
                Assert.Equal(0, sut.PageIndex);
                Assert.True(sut.Paging.NextCommand.CanExecute(null));
                Assert.False(sut.Paging.PreviousCommand.CanExecute(null));
                Assert.False(sut.Paging.FirstCommand.CanExecute(null));
            });

            await Execute(sut.Paging.NextCommand);
            Assert.Equal(1, sut.PageIndex);

            await Execute(sut.Paging.NextCommand);

            Assert.Multiple(() =>
            {
                Assert.Equal(2, sut.PageIndex);
                Assert.False(sut.Paging.NextCommand.CanExecute(null));
                Assert.False(sut.Paging.LastCommand.CanExecute(null));
            });
        }

        [Fact]
        public async Task PagingViewModel_Previous_Does_Step_Back_One_Page()
        {
            var sut = await CreateLoadedDomainList(25);

            await Execute(sut.Paging.LastCommand);
            Assert.Equal(2, sut.PageIndex);

            await Execute(sut.Paging.PreviousCommand);
            Assert.Equal(1, sut.PageIndex);

            await Execute(sut.Paging.PreviousCommand);

            Assert.Multiple(() =>
            {
                Assert.Equal(0, sut.PageIndex);
                Assert.False(sut.Paging.PreviousCommand.CanExecute(null));
            });
        }

        [Fact]
        public async Task PagingViewModel_First_Does_Return_To_Page_Zero()
        {
            var sut = await CreateLoadedDomainList(100);

            await Execute(sut.Paging.LastCommand);
            Assert.Equal(9, sut.PageIndex);

            await Execute(sut.Paging.FirstCommand);
            Assert.Equal(0, sut.PageIndex);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(10)]
        public async Task PagingViewModel_Single_Or_No_Page_Does_Not_Allow_Navigation(int total)
        {
            var sut = await CreateLoadedDomainList(total);

            Assert.Multiple(() =>
            {
                Assert.False(sut.Paging.NextCommand.CanExecute(null));
                Assert.False(sut.Paging.LastCommand.CanExecute(null));
                Assert.False(sut.Paging.PreviousCommand.CanExecute(null));
                Assert.False(sut.Paging.FirstCommand.CanExecute(null));
            });
        }

        [Fact]
        public async Task BusinessSourceList_HasItems_Is_Raised_After_Items_Changed()
        {
            var sut = new SourceList(CreateCommandBuilder());
            var raised = new TaskCompletionSource<(bool HasItems, int Count)>(TaskCreationOptions.RunContinuationsAsynchronously);
            sut.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(SourceList.HasItems))
                {
                    raised.TrySetResult((sut.HasItems, sut.Items.Count));
                }
            };

            await sut.Add(new Entry("a"), TestContext.Current.CancellationToken);

            var (hasItems, count) = await raised.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

            Assert.Multiple(() =>
            {
                Assert.True(hasItems);
                Assert.Equal(1, count);
            });
        }

        [Fact]
        public async Task BusinessSourceList_Unload_And_Dispose_DoNotThrow()
        {
            var sut = new SourceList(CreateCommandBuilder());

            await Execute(sut.LoadCommand);
            Assert.True(sut.CanUnload());

            await Execute(sut.UnloadCommand);
            Assert.False(sut.IsLoaded);

            await Execute(sut.LoadCommand);
            sut.Dispose();
            sut.Dispose();
        }
    }
}
