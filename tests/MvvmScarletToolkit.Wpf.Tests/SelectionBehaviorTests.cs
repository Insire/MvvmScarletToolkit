using Microsoft.Xaml.Behaviors;
using MvvmScarletToolkit.Wpf.Tests.Util;
using System.Collections;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace MvvmScarletToolkit.Wpf.Tests
{
    public sealed record SelectableItem(string Name);

    public sealed class MultiListBoxSelectionBehaviorTests : TraceTestBase
    {
        private readonly SelectableItem _first = new("first");
        private readonly SelectableItem _second = new("second");

        private ListBox CreateListBox()
        {
            return new ListBox
            {
                ItemsSource = new[] { _first, _second },
                SelectionMode = SelectionMode.Multiple,
            };
        }

        [Fact]
        public Task Should_Follow_A_Collection_Set_Before_Attaching()
        {
            return StaRunner.Run(() =>
            {
                var listBox = CreateListBox();
                var selected = new ObservableCollection<object>();
                Interaction.GetBehaviors(listBox).Add(new MultiListBoxSelectionBehavior { SelectedItems = selected });

                selected.Add(_second);

                Assert.Equal([_second], listBox.SelectedItems.Cast<object>());

                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task Detaching_Should_Stop_Following_The_Collection()
        {
            return StaRunner.Run(() =>
            {
                var listBox = CreateListBox();
                var selected = new ObservableCollection<object>();
                var behavior = new MultiListBoxSelectionBehavior();
                Interaction.GetBehaviors(listBox).Add(behavior);

                // set after attaching, like a binding usually does
                behavior.SelectedItems = selected;
                Interaction.GetBehaviors(listBox).Remove(behavior);
                selected.Add(_first);

                Assert.Empty(listBox.SelectedItems);

                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task Should_Ignore_Selection_Changes_Of_Nested_Selectors()
        {
            return StaRunner.Run(async () =>
            {
                var nested = new ComboBox();
                var listBox = new ListBox { SelectionMode = SelectionMode.Multiple };
                listBox.Items.Add(nested);

                var selected = new ObservableCollection<object>();
                Interaction.GetBehaviors(listBox).Add(new MultiListBoxSelectionBehavior { SelectedItems = selected });

                var window = Ui.Show(listBox);
                await Ui.Settle();

                nested.RaiseEvent(new SelectionChangedEventArgs(Selector.SelectionChangedEvent, new ArrayList(), new ArrayList { "nested item" }));

                Assert.Empty(selected);

                listBox.SelectedItems.Add(nested);
                Assert.Equal([nested], selected);
                window.Close();
            });
        }
    }

    public sealed class MultiSelectionBehaviorTests : TraceTestBase
    {
        private readonly SelectableItem _first = new("first");
        private readonly SelectableItem _second = new("second");

        private DataGrid CreateDataGrid()
        {
            return new DataGrid
            {
                ItemsSource = new[] { _first, _second },
                SelectionMode = DataGridSelectionMode.Extended,
            };
        }

        [Fact]
        public Task Should_Follow_A_Collection_Set_Before_Attaching()
        {
            return StaRunner.Run(() =>
            {
                var dataGrid = CreateDataGrid();
                var selected = new ObservableCollection<object>();
                Interaction.GetBehaviors(dataGrid).Add(new MultiSelectionBehavior { SelectedItems = selected });

                selected.Add(_second);

                Assert.Equal([_second], dataGrid.SelectedItems.Cast<object>());

                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task Detaching_Should_Stop_Following_The_Collection()
        {
            return StaRunner.Run(() =>
            {
                var dataGrid = CreateDataGrid();
                var selected = new ObservableCollection<object>();
                var behavior = new MultiSelectionBehavior();
                Interaction.GetBehaviors(dataGrid).Add(behavior);

                // set after attaching, like a binding usually does
                behavior.SelectedItems = selected;
                Interaction.GetBehaviors(dataGrid).Remove(behavior);
                selected.Add(_first);

                Assert.Empty(dataGrid.SelectedItems);

                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task Should_Write_Selection_Back_To_The_Collection()
        {
            return StaRunner.Run(() =>
            {
                var dataGrid = CreateDataGrid();
                var selected = new ObservableCollection<object>();
                Interaction.GetBehaviors(dataGrid).Add(new MultiSelectionBehavior { SelectedItems = selected });

                dataGrid.SelectedItems.Add(_first);

                Assert.Equal([_first], selected);

                return Task.CompletedTask;
            });
        }
    }
}
