using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace MvvmScarletToolkit
{
    // usage:
    // xmlns:i="http://schemas.microsoft.com/expression/2010/interactivity"
    // <i:Interaction.Behaviors>
    //  <local:SelectedTreeViewItemBehavior SelectedItem = "{Binding SelectedItem, Mode=TwoWay}" />
    // </ i:Interaction.Behaviors>
    /// <summary>
    /// Provides  oneway readonly Binding Support for the Treeview control to support binding to a selected item,
    /// </summary>
    /// <seealso cref="System.Windows.Interactivity.Behavior{TreeView}" />
    public class SelectedTreeViewItemBehavior : Behavior<TreeView>
    {
        public object SelectedItem
        {
            get { return GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        /// <summary>Identifies the <see cref="SelectedItem"/> dependency property.</summary>
        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
            nameof(SelectedItem),
            typeof(object),
            typeof(SelectedTreeViewItemBehavior),
            new UIPropertyMetadata(default, OnSelectedItemChanged));

        private static void OnSelectedItemChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var item = e.NewValue as TreeViewItem;

            item?.SetCurrentValue(TreeViewItem.IsSelectedProperty, true);
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            if (AssociatedObject != null)
                AssociatedObject.SelectedItemChanged += OnTreeViewSelectedItemChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            if (AssociatedObject != null)
                AssociatedObject.SelectedItemChanged -= OnTreeViewSelectedItemChanged;
        }

        private void OnTreeViewSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            SetCurrentValue(SelectedItemProperty, e.NewValue);
        }
    }
}
