using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace MvvmScarletToolkit.Wpf
{
    /// <summary>
    /// Behavior that opens a context menu on the column headers of a <see cref="DataGrid"/>, to hide and show its columns. The last visible column can't be hidden.
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
    //    <mvvm:DataGridColumnChooserBehavior />
    // </ i:Interaction.Behaviors>
    public sealed class DataGridColumnChooserBehavior : Behavior<DataGrid>
    {
        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.AddHandler(FrameworkElement.ContextMenuOpeningEvent, new ContextMenuEventHandler(OnContextMenuOpening));
        }

        protected override void OnDetaching()
        {
            AssociatedObject.RemoveHandler(FrameworkElement.ContextMenuOpeningEvent, new ContextMenuEventHandler(OnContextMenuOpening));

            base.OnDetaching();
        }

        private void OnContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            var header = (e.OriginalSource as DependencyObject)?.FindParent<DataGridColumnHeader>();
            if (header is null)
            {
                return;
            }

            e.Handled = true;

            var menu = CreateMenu();
            menu.PlacementTarget = header;
            menu.IsOpen = true;
        }

        internal ContextMenu CreateMenu()
        {
            var menu = new ContextMenu();
            var columns = AssociatedObject.Columns.OrderBy(p => p.DisplayIndex).ToArray();
            var isLastVisible = columns.Count(IsVisible) == 1;

            for (var i = 0; i < columns.Length; i++)
            {
                var column = columns[i];
                var item = new MenuItem
                {
                    Header = column.GetCaption(i),
                    IsCheckable = true,
                    IsChecked = IsVisible(column),
                    IsEnabled = !(isLastVisible && IsVisible(column)),
                    StaysOpenOnClick = true,
                };

                item.Click += (_, _) =>
                {
                    column.Visibility = item.IsChecked ? Visibility.Visible : Visibility.Collapsed;
                    UpdateEnabled(menu, columns);
                };

                menu.Items.Add(item);
            }

            return menu;
        }

        private static void UpdateEnabled(ContextMenu menu, DataGridColumn[] columns)
        {
            var isLastVisible = columns.Count(IsVisible) == 1;

            for (var i = 0; i < columns.Length; i++)
            {
                ((MenuItem)menu.Items[i]).IsEnabled = !(isLastVisible && IsVisible(columns[i]));
            }
        }

        private static bool IsVisible(DataGridColumn column)
        {
            return column.Visibility == Visibility.Visible;
        }
    }
}
