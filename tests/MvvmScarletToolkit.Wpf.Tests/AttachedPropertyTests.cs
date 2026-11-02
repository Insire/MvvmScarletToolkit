using MvvmScarletToolkit.Wpf.Tests.Util;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace MvvmScarletToolkit.Wpf.Tests
{
    public sealed class FilterTests : TraceTestBase
    {
        private static ItemsControl CreateItemsControl()
        {
            return new ItemsControl { ItemsSource = new List<int> { 1, 2, 3, 4, 5 } };
        }

        private static int Count(System.ComponentModel.ICollectionView view) => view.Cast<object>().Count();

        [Fact]
        public Task Should_Filter_ItemsControl()
        {
            return StaRunner.Run(() =>
            {
                var itemsControl = CreateItemsControl();

                Filter.SetBy(itemsControl, p => (int)p > 2);

                Assert.Equal(3, itemsControl.Items.Count);

                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task Replacing_The_Predicate_Should_Stop_Calling_The_Old_One()
        {
            return StaRunner.Run(() =>
            {
                var itemsControl = CreateItemsControl();
                var oldCalls = 0;

                Filter.SetBy(itemsControl, _ => { oldCalls++; return true; });
                Filter.SetBy(itemsControl, p => (int)p < 3);

                oldCalls = 0;
                itemsControl.Items.Refresh();

                Assert.Multiple(() =>
                {
                    Assert.Equal(0, oldCalls);
                    Assert.Equal(2, itemsControl.Items.Count);
                });

                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task Clearing_The_Predicate_Should_Remove_The_Filter()
        {
            return StaRunner.Run(() =>
            {
                var itemsControl = CreateItemsControl();
                Filter.SetBy(itemsControl, p => (int)p > 2);

                itemsControl.ClearValue(Filter.ByProperty);

                Assert.Multiple(() =>
                {
                    Assert.Null(itemsControl.Items.Filter);
                    Assert.Equal(5, itemsControl.Items.Count);
                });

                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task Clearing_The_Predicate_Should_Keep_A_Filter_Set_By_Someone_Else()
        {
            return StaRunner.Run(() =>
            {
                var itemsControl = CreateItemsControl();
                Filter.SetBy(itemsControl, p => (int)p > 2);

                Predicate<object> foreign = p => (int)p == 1;
                itemsControl.Items.Filter = foreign;
                itemsControl.ClearValue(Filter.ByProperty);

                Assert.Same(foreign, itemsControl.Items.Filter);

                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task Should_Filter_CollectionViewSource_Once_Its_View_Exists()
        {
            return StaRunner.Run(() =>
            {
                var collectionViewSource = new CollectionViewSource();
                Filter.SetBy(collectionViewSource, p => (int)p > 2);

                collectionViewSource.Source = new List<int> { 1, 2, 3, 4, 5 };
                Assert.Equal(3, Count(collectionViewSource.View));

                // a new source gets a new view
                collectionViewSource.Source = new List<int> { 1, 5 };
                Assert.Equal(1, Count(collectionViewSource.View));

                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task Clearing_The_Predicate_Should_Remove_The_Filter_Of_CollectionViewSource()
        {
            return StaRunner.Run(() =>
            {
                var collectionViewSource = new CollectionViewSource { Source = new List<int> { 1, 2, 3, 4, 5 } };
                Filter.SetBy(collectionViewSource, p => (int)p > 2);
                Assert.Equal(3, Count(collectionViewSource.View));

                collectionViewSource.ClearValue(Filter.ByProperty);
                Assert.Equal(5, Count(collectionViewSource.View));

                // no longer applied to views created later
                collectionViewSource.Source = new List<int> { 1, 5 };
                Assert.Equal(2, Count(collectionViewSource.View));

                return Task.CompletedTask;
            });
        }
    }

    public sealed class FocusTests : TraceTestBase
    {
        [Fact]
        public Task First_Should_Focus_The_First_Element_When_Loaded()
        {
            return StaRunner.Run(async () =>
            {
                var button = new Button();
                var panel = new StackPanel { Children = { button, new Button() } };
                Focus.SetFirst(panel, true);

                var window = Ui.Show(panel);
                await Ui.Settle();

                Assert.Same(button, FocusManager.GetFocusedElement(window));
                window.Close();
            });
        }

        [Fact]
        public Task Disabling_First_Should_Stop_Moving_Focus()
        {
            return StaRunner.Run(async () =>
            {
                var panel = new StackPanel { Children = { new Button(), new Button() } };
                Focus.SetFirst(panel, true);
                Focus.SetFirst(panel, false);

                var window = Ui.Show(panel);
                await Ui.Settle();

                Assert.Null(FocusManager.GetFocusedElement(window));
                window.Close();
            });
        }
    }

    public sealed class DependencyObjectExtensionsTests : TraceTestBase
    {
        [Fact]
        public Task FindTopMostParent_Should_Return_The_Root_Of_The_Tree()
        {
            return StaRunner.Run(async () =>
            {
                var button = new Button();
                var window = Ui.Show(new Grid { Children = { new StackPanel { Children = { button } } } });
                await Ui.Settle();

                Assert.Same(window, button.FindTopMostParent());
                window.Close();
            });
        }

        [Fact]
        public Task FindTopMostParent_Should_Return_The_Element_Without_Parent()
        {
            return StaRunner.Run(() =>
            {
                var button = new Button();

                Assert.Same(button, button.FindTopMostParent());

                return Task.CompletedTask;
            });
        }

        [Fact]
        public void FindTopMostParent_Should_Return_Null_For_Null()
        {
            Assert.Null(((DependencyObject?)null).FindTopMostParent());
        }

        [Fact]
        public Task FindDataTemplateFor_Should_Return_Null_When_Missing()
        {
            return StaRunner.Run(() =>
            {
                Assert.Null(new Border().FindDataTemplateFor(typeof(SelectableItem)));

                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task FindDataTemplateFor_Should_Find_Template_In_Resources()
        {
            return StaRunner.Run(() =>
            {
                var template = new DataTemplate(typeof(SelectableItem));
                var border = new Border();
                border.Resources.Add(new DataTemplateKey(typeof(SelectableItem)), template);

                Assert.Same(template, border.FindDataTemplateFor(typeof(SelectableItem)));

                return Task.CompletedTask;
            });
        }
    }

    public sealed class TypeDataTemplateSelectorTests : TraceTestBase
    {
        [Fact]
        public Task Should_Return_Null_For_Null_Item()
        {
            return StaRunner.Run(() =>
            {
                Assert.Null(new TypeDataTemplateSelector().SelectTemplate(null!, new Border()));

                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task Should_Prefer_Its_Own_Templates()
        {
            return StaRunner.Run(() =>
            {
                var template = new DataTemplate(typeof(SelectableItem));
                var selector = new TypeDataTemplateSelector();
                selector.Templates.Add(template);

                Assert.Same(template, selector.SelectTemplate(new SelectableItem("a"), new Border()));

                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task Should_Fall_Back_To_Resources()
        {
            return StaRunner.Run(() =>
            {
                var template = new DataTemplate(typeof(SelectableItem));
                var border = new Border();
                border.Resources.Add(new DataTemplateKey(typeof(SelectableItem)), template);

                Assert.Same(template, new TypeDataTemplateSelector().SelectTemplate(new SelectableItem("a"), border));

                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task Should_Return_Null_When_No_Template_Exists()
        {
            return StaRunner.Run(() =>
            {
                Assert.Null(new TypeDataTemplateSelector().SelectTemplate(new SelectableItem("a"), new Border()));

                return Task.CompletedTask;
            });
        }
    }
}
