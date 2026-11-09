using Microsoft.Xaml.Behaviors;
using MvvmScarletToolkit.Wpf.Tests.Util;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;

namespace MvvmScarletToolkit.Wpf.Tests
{
    public sealed class LaunchNavigateUriAsNewProcessBehaviorTests : TraceTestBase
    {
        private static (Hyperlink Link, LaunchNavigateUriAsNewProcessBehavior Behavior) CreateLink()
        {
            var link = new Hyperlink();
            var behavior = new LaunchNavigateUriAsNewProcessBehavior();
            Interaction.GetBehaviors(link).Add(behavior);

            return (link, behavior);
        }

        [Fact]
        public Task Should_Only_Be_Executable_With_A_Uri()
        {
            return StaRunner.Run(() =>
            {
                var (link, _) = CreateLink();

                var withoutUri = link.Command.CanExecute(null);
                link.NavigateUri = new Uri("https://example.com");

                Assert.Multiple(() =>
                {
                    Assert.False(withoutUri);
                    Assert.True(link.Command.CanExecute(null));
                });

                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task Should_Requery_The_Command_When_The_Uri_Changes()
        {
            return StaRunner.Run(() =>
            {
                var (link, _) = CreateLink();
                var count = 0;
                link.Command.CanExecuteChanged += (_, _) => count++;

                link.NavigateUri = new Uri("https://example.com");

                Assert.Equal(1, count);

                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task Should_Not_Start_Anything_Without_A_Uri()
        {
            return StaRunner.Run(() =>
            {
                var (link, _) = CreateLink();

                link.Command.Execute(null);

                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task Should_Remove_The_Command_When_Detached()
        {
            return StaRunner.Run(() =>
            {
                var (link, behavior) = CreateLink();
                var command = link.Command;
                var count = 0;
                command.CanExecuteChanged += (_, _) => count++;

                Interaction.GetBehaviors(link).Remove(behavior);
                link.NavigateUri = new Uri("https://example.com");

                Assert.Multiple(() =>
                {
                    Assert.Null(link.Command);
                    Assert.Equal(0, count);
                });

                return Task.CompletedTask;
            });
        }
    }

    public sealed class SelectedTreeViewItemBehaviorTests : TraceTestBase
    {
        [Fact]
        public Task Should_Follow_The_Selection_Of_The_TreeView()
        {
            return StaRunner.Run(async () =>
            {
                var item = new TreeViewItem { Header = "item" };
                var treeView = new TreeView { Items = { item } };
                var behavior = new SelectedTreeViewItemBehavior();
                Interaction.GetBehaviors(treeView).Add(behavior);

                var window = Ui.Show(treeView);
                await Ui.Settle();

                item.IsSelected = true;

                Assert.Same(item, behavior.SelectedItem);
                window.Close();
            });
        }

        [Fact]
        public Task Should_Stop_Following_The_Selection_When_Detached()
        {
            return StaRunner.Run(async () =>
            {
                var item = new TreeViewItem { Header = "item" };
                var treeView = new TreeView { Items = { item } };
                var behavior = new SelectedTreeViewItemBehavior();
                Interaction.GetBehaviors(treeView).Add(behavior);

                var window = Ui.Show(treeView);
                await Ui.Settle();

                Interaction.GetBehaviors(treeView).Remove(behavior);
                item.IsSelected = true;

                Assert.Null(behavior.SelectedItem);
                window.Close();
            });
        }
    }

    public sealed class SmoothProgressBehaviorTests : TraceTestBase
    {
        /// <summary>
        /// Checks that the value is animated instead of set. Waiting for the animation to reach its end depends on
        /// WPF's animation clock, which only ticks with rendering and stalls on the hidden desktop under parallel load.
        /// </summary>
        [Fact]
        public Task Should_Animate_The_Value_Instead_Of_Setting_It()
        {
            return StaRunner.Run(() =>
            {
                var progressBar = new ProgressBar { Maximum = 100 };
                var behavior = new SmoothProgressBehavior();
                Interaction.GetBehaviors(progressBar).Add(behavior);

                var animatedBefore = progressBar.HasAnimatedProperties;

                behavior.Percentage = 80;

                Assert.Multiple(() =>
                {
                    Assert.False(animatedBefore);
                    Assert.True(progressBar.HasAnimatedProperties);
                    Assert.Equal(0d, (double)progressBar.GetAnimationBaseValue(RangeBase.ValueProperty));
                    Assert.Equal(80, behavior.Percentage);
                });

                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task Should_Ignore_Changes_While_Not_Attached()
        {
            return StaRunner.Run(() =>
            {
                var behavior = new SmoothProgressBehavior { Percentage = 80 };

                Assert.Equal(80, behavior.Percentage);

                return Task.CompletedTask;
            });
        }
    }
}
