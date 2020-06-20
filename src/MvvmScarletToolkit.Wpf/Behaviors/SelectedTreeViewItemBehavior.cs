using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Controls;

namespace MvvmScarletToolkit
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
        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.SelectedItemChanged += OnTreeViewSelectedItemChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            if (AssociatedObject is null)
            {
                return;
            }

            AssociatedObject.SelectedItemChanged -= OnTreeViewSelectedItemChanged;
        }

        private void OnTreeViewSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            SetCurrentValue(SelectedItemProperty, e.NewValue);
        }

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
             new FrameworkPropertyMetadata(default, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
    }
}
