using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using System.Windows.Media;

namespace MvvmScarletToolkit.FileSystemBrowser
{
    /// <summary>
    /// Behavior that enables twoway binding to the selected item property of a WPF treeview
    /// </summary>
    /// <remarks>
    /// requires the xmlns:i="http://schemas.microsoft.com/expression/2010/interactivity" namespace
    /// </remarks>
    /// <example>
    /// Here is how to use the behavior in XAML:
    /// <code>
    /// <i:Interaction.Behaviors>
    ///     <local:SelectedTreeViewItemBehavior SelectedItem = "{Binding SomeViewModel.SomeProperty}" />
    /// </ i:Interaction.Behaviors>
    /// </code>
    /// </example>
    // source: https://stackoverflow.com/questions/11065995/binding-selecteditem-in-a-hierarchicaldatatemplate-applied-wpf-treeview/18700099#18700099
    public sealed class SelectedTreeViewItemBehavior : Behavior<TreeView>
    {
        public object SelectedItem
        {
            get { return GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
            nameof(SelectedItem),
            typeof(object),
            typeof(SelectedTreeViewItemBehavior),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedItemChanged));

        private static void OnSelectedItemChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is TreeViewItem item)
            {
                item.SetValue(TreeViewItem.IsSelectedProperty, true);
                return;
            }

            var behavior = (SelectedTreeViewItemBehavior)sender;
            var treeView = behavior.AssociatedObject;
            if (treeView is null)
            {
                // at designtime the AssociatedObject sometimes seems to be null
                return;
            }

            item = GetTreeViewItem(treeView, e.NewValue);
            if (item is null)
                return;

            item.IsSelected = true;
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.SelectedItemChanged += OnTreeViewSelectedItemChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            if (AssociatedObject is null)
                return;

            AssociatedObject.SelectedItemChanged -= OnTreeViewSelectedItemChanged;
        }

        private void OnTreeViewSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            SelectedItem = e.NewValue;
        }

        private static Action<int> GetBringIndexIntoView(Panel itemsHostPanel)
        {
            if (!(itemsHostPanel is VirtualizingStackPanel virtualizingPanel))
                return null;

            var method = virtualizingPanel.GetType().GetMethod(
                "BringIndexIntoView",
                BindingFlags.Instance | BindingFlags.NonPublic,
                Type.DefaultBinder,
                new[] { typeof(int) },
                null);

            if (method is null)
                return null;

            return i => method.Invoke(virtualizingPanel, new object[] { i });
        }

        /// <summary>
        /// Recursively search for an item in this subtree.
        /// </summary>
        /// <param name="container">
        /// The parent ItemsControl. This can be a TreeView or a TreeViewItem.
        /// </param>
        /// <param name="item">
        /// The item to search for.
        /// </param>
        /// <returns>
        /// The TreeViewItem that contains the specified item.
        /// </returns>
        private static TreeViewItem GetTreeViewItem(ItemsControl container, object item)
        {
            if (container is null)
                return null;

            if (container.DataContext == item)
                return container as TreeViewItem;

            // Expand the current container
            if (container is TreeViewItem treeViewItem && !treeViewItem.IsExpanded)
                container.SetValue(TreeViewItem.IsExpandedProperty, true);

            // Try to generate the ItemsPresenter and the ItemsPanel.
            // by calling ApplyTemplate.  Note that in the
            // virtualizing case even if the item is marked
            // expanded we still need to do this step in order to
            // regenerate the visuals because they may have been virtualized away.
            container.ApplyTemplate();
            var itemsPresenter = (ItemsPresenter)container.Template.FindName("ItemsHost", container);
            if (itemsPresenter != null)
            {
                itemsPresenter.ApplyTemplate();
            }
            else
            {
                // The Tree template has not named the ItemsPresenter,
                // so walk the descendents and find the child.
                itemsPresenter = container.GetVisualDescendant<ItemsPresenter>();
                if (itemsPresenter is null)
                {
                    container.UpdateLayout();
                    itemsPresenter = container.GetVisualDescendant<ItemsPresenter>();
                }
            }

            var itemsHostPanel = (Panel)VisualTreeHelper.GetChild(itemsPresenter, 0);

            // Ensure that the generator for this panel has been created.
#pragma warning disable 168
            var children = itemsHostPanel.Children;
#pragma warning restore 168

            var bringIndexIntoView = GetBringIndexIntoView(itemsHostPanel);
            for (int i = 0, count = container.Items.Count; i < count; i++)
            {
                TreeViewItem subContainer;
                if (bringIndexIntoView != null)
                {
                    // Bring the item into view so
                    // that the container will be generated.
                    bringIndexIntoView(i);
                    subContainer = (TreeViewItem)container.ItemContainerGenerator.ContainerFromIndex(i);
                }
                else
                {
                    subContainer = (TreeViewItem)container.ItemContainerGenerator.ContainerFromIndex(i);

                    // Bring the item into view to maintain the
                    // same behavior as with a virtualizing panel.
                    subContainer.BringIntoView();
                }

                if (subContainer is null)
                    continue;

                // Search the next level for the object.
                var resultContainer = GetTreeViewItem(subContainer, item);
                if (resultContainer != null)
                    return resultContainer;

                // The object is not under this TreeViewItem
                // so collapse it.
                subContainer.IsExpanded = false;
            }

            return null;
        }
    }

    public static class DependencyObjectExtensions
    {
        /// <summary>
        ///     Gets the first child of the specified visual that is of tyoe <typeparamref name="T" />
        ///     in the visual tree recursively.
        /// </summary>
        /// <param name="visual">The visual to get the visual children for.</param>
        /// <returns>
        ///     The first child of the specified visual that is of tyoe <typeparamref name="T" /> of the
        ///     specified visual in the visual tree recursively or <c>null</c> if none was found.
        /// </returns>
        public static T GetVisualDescendant<T>(this DependencyObject visual) where T : DependencyObject
        {
            return (T)visual.GetVisualDescendants().FirstOrDefault(d => d is T);
        }

        /// <summary>
        ///     Gets all children of the specified visual in the visual tree recursively.
        /// </summary>
        /// <param name="visual">The visual to get the visual children for.</param>
        /// <returns>All children of the specified visual in the visual tree recursively.</returns>
        public static IEnumerable<DependencyObject> GetVisualDescendants(this DependencyObject visual)
        {
            if (visual is null)
                yield break;

            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(visual); i++)
            {
                var child = VisualTreeHelper.GetChild(visual, i);
                yield return child;
                foreach (var subChild in GetVisualDescendants(child))
                {
                    yield return subChild;
                }
            }
        }
    }
}
