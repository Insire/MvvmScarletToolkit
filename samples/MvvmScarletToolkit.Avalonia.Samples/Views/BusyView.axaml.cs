using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using MvvmScarletToolkit.Core.Samples.Features.Busy;
using System.ComponentModel;

namespace MvvmScarletToolkit.Avalonia.Samples.Views
{
    public partial class BusyView : UserControl
    {
        public BusyView()
        {
            InitializeComponent();
        }

        private void TreeMenu_Opening(object? sender, CancelEventArgs e)
        {
            // this menu is for the empty area, nodes have their own. Children can't have children, so there is nothing to offer for a selected one
            if (Tree.SelectedItem is ObservableBusyViewModel || Tree.GetVisualDescendants().OfType<TreeViewItem>().Any(p => p.IsPointerOver))
            {
                e.Cancel = true;
                return;
            }

            // add to the selected container, or to the root without one
            if (sender is ContextMenu menu)
            {
                menu.DataContext = Tree.SelectedItem as BusyViewModel ?? DataContext;
            }
        }

        private void Tree_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            // otherwise a selection could not be undone, and the root would be out of reach for the context menu
            if (e.GetCurrentPoint(Tree).Properties.IsLeftButtonPressed && (e.Source as Control)?.FindAncestorOfType<TreeViewItem>(includeSelf: true) is null)
            {
                Tree.SelectedItem = null;
            }
        }
    }
}
