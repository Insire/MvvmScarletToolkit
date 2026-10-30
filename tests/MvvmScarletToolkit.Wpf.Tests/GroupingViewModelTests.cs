using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using MvvmScarletToolkit.Observables;
using MvvmScarletToolkit.Wpf.Tests.Util;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows.Data;
using System.Windows.Threading;

namespace MvvmScarletToolkit.Wpf.Tests
{
    public sealed partial class GroupingViewModelTests : TraceTestBase
    {
        private sealed partial class Row : ViewModelBase
        {
            [ObservableProperty]
            public partial string? Color { get; set; }

            [ObservableProperty]
            public partial string? Size { get; set; }

            public Row(IScarletCommandBuilder commandBuilder)
                : base(commandBuilder)
            {
            }
        }

        private static IScarletCommandBuilder CreateCommandBuilder()
        {
            return Utils.GetTestCommandBuilder(new ScarletDispatcher(Dispatcher.CurrentDispatcher), messenger: new WeakReferenceMessenger());
        }

        private static GroupingViewModel Create(IScarletCommandBuilder commandBuilder, out ListCollectionView view, GroupingViewModelOptions? options = null)
        {
            var collectionView = new ListCollectionView(new ObservableCollection<Row>());
            view = collectionView;

            return GroupingViewModel.Create<Row>(commandBuilder, () => collectionView, options);
        }

        private static string[] GetGroups(ListCollectionView view)
        {
            return view.GroupDescriptions.Cast<PropertyGroupDescription>().Select(p => p.PropertyName).ToArray();
        }

        private static async Task Idle()
        {
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
        }

        [Fact]
        public Task GroupBy_Should_Load_And_Add_The_Group_Description()
        {
            return StaRunner.Run(async () =>
            {
                var sut = Create(CreateCommandBuilder(), out var view);

                await sut.GroupBy(nameof(Row.Color), CancellationToken.None);
                await Idle();

                Assert.True(sut.IsLoaded);
                Assert.True(sut.IsGroupedBy(nameof(Row.Color)));
                Assert.Equal([nameof(Row.Color)], GetGroups(view));
            });
        }

        [Fact]
        public Task GroupBy_Should_Ignore_An_Active_Grouping()
        {
            return StaRunner.Run(async () =>
            {
                var sut = Create(CreateCommandBuilder(), out var view);

                await sut.GroupBy(nameof(Row.Color), CancellationToken.None);
                await Idle();
                await sut.GroupBy(nameof(Row.Color), CancellationToken.None);
                await Idle();

                Assert.Equal(1, sut.Count);
                Assert.Equal([nameof(Row.Color)], GetGroups(view));
            });
        }

        [Fact]
        public Task GroupBy_Should_Keep_The_Order_Of_Calls()
        {
            return StaRunner.Run(async () =>
            {
                var sut = Create(CreateCommandBuilder(), out var view);

                await sut.GroupBy(nameof(Row.Size), CancellationToken.None);
                await Idle();
                await sut.GroupBy(nameof(Row.Color), CancellationToken.None);
                await Idle();

                Assert.Equal([nameof(Row.Size), nameof(Row.Color)], GetGroups(view));
            });
        }

        [Fact]
        public Task GroupBy_Should_Reject_Unknown_Properties()
        {
            return StaRunner.Run(async () =>
            {
                var sut = Create(CreateCommandBuilder(), out _);

                await Assert.ThrowsAsync<ArgumentException>(() => sut.GroupBy("Unknown", CancellationToken.None));
            });
        }

        [Fact]
        public Task Ungroup_Should_Remove_The_Group_Description_And_Offer_It_Again()
        {
            return StaRunner.Run(async () =>
            {
                var sut = Create(CreateCommandBuilder(), out var view);

                await sut.GroupBy(nameof(Row.Color), CancellationToken.None);
                await Idle();
                await sut.GroupBy(nameof(Row.Size), CancellationToken.None);
                await Idle();

                await sut.Ungroup(nameof(Row.Color), CancellationToken.None);
                await Idle();

                Assert.False(sut.IsGroupedBy(nameof(Row.Color)));
                Assert.Equal([nameof(Row.Size)], GetGroups(view));

                await sut.GroupBy(nameof(Row.Color), CancellationToken.None);
                await Idle();

                Assert.Equal([nameof(Row.Size), nameof(Row.Color)], GetGroups(view));
            });
        }

        [Fact]
        public Task Ungroup_Should_Ignore_Inactive_Groupings()
        {
            return StaRunner.Run(async () =>
            {
                var sut = Create(CreateCommandBuilder(), out var view);

                await sut.Ungroup(nameof(Row.Color), CancellationToken.None);

                Assert.Empty(GetGroups(view));
            });
        }

        [Fact]
        public Task Instances_Sharing_A_Messenger_Should_Not_Affect_Each_Other()
        {
            return StaRunner.Run(async () =>
            {
                var commandBuilder = CreateCommandBuilder();
                var first = Create(commandBuilder, out var firstView);
                var second = Create(commandBuilder, out var secondView);

                await first.GroupBy(nameof(Row.Color), CancellationToken.None);
                await Idle();
                await second.GroupBy(nameof(Row.Color), CancellationToken.None);
                await Idle();
                await first.Ungroup(nameof(Row.Color), CancellationToken.None);
                await Idle();

                Assert.Empty(GetGroups(firstView));
                Assert.Equal([nameof(Row.Color)], GetGroups(secondView));
                Assert.True(second.IsGroupedBy(nameof(Row.Color)));
            });
        }

        [Fact]
        public Task A_Selected_Group_Should_Stay_Usable_After_Other_Instances_Dropped_It()
        {
            return StaRunner.Run(async () =>
            {
                var sut = Create(CreateCommandBuilder(), out _);

                await sut.GroupBy(nameof(Row.Size), CancellationToken.None);
                await Idle();
                await sut.GroupBy(nameof(Row.Color), CancellationToken.None);
                await Idle();

                var selected = sut[1].SelectedItem!;
                var isDisposed = typeof(ViewModelBase).GetProperty("IsDisposed", BindingFlags.Instance | BindingFlags.NonPublic)!;

                Assert.False((bool)isDisposed.GetValue(selected)!);
                Assert.DoesNotContain(selected, sut[0].Items);
            });
        }

        [Fact]
        public Task Removed_Instances_Should_Not_React_To_Later_Selections()
        {
            return StaRunner.Run(async () =>
            {
                var sut = Create(CreateCommandBuilder(), out var view);

                await sut.GroupBy(nameof(Row.Size), CancellationToken.None);
                await Idle();
                var removed = sut[0];

                await sut.Ungroup(nameof(Row.Size), CancellationToken.None);
                await Idle();
                await sut.GroupBy(nameof(Row.Color), CancellationToken.None);
                await Idle();

                // StaRunner fails the test when a handler of the disposed instance throws
                Assert.Equal([nameof(Row.Color)], GetGroups(view));
                Assert.Null(removed.Items.FirstOrDefault());
            });
        }

        [Fact]
        public Task Without_Options_Every_Public_Property_Should_Be_Offered()
        {
            return StaRunner.Run(async () =>
            {
                var sut = Create(CreateCommandBuilder(), out _);

                await sut.GroupBy(nameof(ViewModelBase.IsBusy), CancellationToken.None);
                await Idle();

                Assert.True(sut.IsGroupedBy(nameof(ViewModelBase.IsBusy)));
            });
        }

        [Fact]
        public Task ExcludeToolkitProperties_Should_Hide_Base_Class_Properties()
        {
            return StaRunner.Run(async () =>
            {
                var sut = Create(CreateCommandBuilder(), out _, new GroupingViewModelOptions { ExcludeToolkitProperties = true });

                await sut.GroupBy(nameof(Row.Color), CancellationToken.None);
                await Idle();

                Assert.Equal([nameof(Row.Color), nameof(Row.Size)], sut[0].Items.Concat([sut[0].SelectedItem!]).Select(p => p.Name).Distinct().Order());
                await Assert.ThrowsAsync<ArgumentException>(() => sut.GroupBy(nameof(ViewModelBase.IsBusy), CancellationToken.None));
            });
        }

        [Fact]
        public Task Properties_Should_Limit_What_Is_Offered()
        {
            return StaRunner.Run(async () =>
            {
                var sut = Create(CreateCommandBuilder(), out _, new GroupingViewModelOptions { Properties = [nameof(Row.Size)] });

                await sut.GroupBy(nameof(Row.Size), CancellationToken.None);
                await Idle();

                await Assert.ThrowsAsync<ArgumentException>(() => sut.GroupBy(nameof(Row.Color), CancellationToken.None));
            });
        }
    }
}
