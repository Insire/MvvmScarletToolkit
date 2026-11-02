using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace MvvmScarletToolkit.Wpf
{
    /// <summary>
    /// Behavior that enables scrolling a SelectedItem of a <see cref="ListBox"/>, <see cref="DataGrid"/> or <see cref="TreeView"/> into the View
    /// </summary>
    /// <remarks>
    /// required namespaces:
    /// <list type="bullet">
    /// <item>
    /// <description>xmlns:i="http://schemas.microsoft.com/xaml/behaviors"</description>
    /// </item>
    /// <item>
    /// <description>xmlns:mvvm="http://SoftThorn.MvvmScarletToolkit.com/winfx/xaml/shared"</description>
    /// </item>
    /// </list>
    /// </remarks>
    // usage:
    // <i:Interaction.Behaviors>
    //    <mvvm:ScrollSelectionIntoViewBehavior />
    // </ i:Interaction.Behaviors>
    public sealed class ScrollSelectionIntoViewBehavior : Behavior<ItemsControl>
    {
        private const string TreeViewItemHeaderPart = "PART_Header";

        protected override void OnAttached()
        {
            base.OnAttached();

            switch (AssociatedObject)
            {
                case Selector selector:
                    selector.SelectionChanged -= OnSelectionChanged;
                    selector.SelectionChanged += OnSelectionChanged;
                    break;

                case TreeView treeView:
                    treeView.AddHandler(TreeViewItem.SelectedEvent, new RoutedEventHandler(OnTreeViewItemSelected));
                    break;
            }
        }

        private static void OnTreeViewItemSelected(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is not TreeViewItem item)
            {
                return;
            }

            // deferred, so that the selection change completes before scrolling
            item.Dispatcher.InvokeAsync(ScrollIntoView);

            void ScrollIntoView()
            {
                item.UpdateLayout();

                // the item contains its children, which can exceed the viewport and leave the selected row out of view
                var target = item.Template?.FindName(TreeViewItemHeaderPart, item) as FrameworkElement ?? item;

                // a narrow rectangle, wide rows would otherwise scroll horizontally and hide the expanders
                target.BringIntoView(new Rect(0, 0, 1, target.ActualHeight));
            }
        }

        private static void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (sender)
            {
                case DataGrid { SelectedItem: null }:
                    return;

                case DataGrid dataGrid:
                {
                    var selectedItem = dataGrid.SelectedItem;

                    // deferred, so that the selection change completes before scrolling
                    dataGrid.Dispatcher.InvokeAsync(ScrollIntoView);
                    return;

                    void ScrollIntoView()
                    {
                        dataGrid.UpdateLayout();

                        dataGrid.ScrollIntoView(selectedItem);
                    }
                }

                case ListBox { SelectedItem: null }:
                    return;

                case ListBox listBox:
                {
                    var selectedItem = listBox.SelectedItem;

                    // deferred, so that the selection change completes before scrolling
                    listBox.Dispatcher.InvokeAsync(ScrollIntoView);
                    return;

                    void ScrollIntoView()
                    {
                        listBox.UpdateLayout();

                        listBox.ScrollIntoView(selectedItem);
                    }
                }
            }
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            switch (AssociatedObject)
            {
                case Selector selector:
                    selector.SelectionChanged -= OnSelectionChanged;
                    break;

                case TreeView treeView:
                    treeView.RemoveHandler(TreeViewItem.SelectedEvent, new RoutedEventHandler(OnTreeViewItemSelected));
                    break;
            }
        }
    }
}
