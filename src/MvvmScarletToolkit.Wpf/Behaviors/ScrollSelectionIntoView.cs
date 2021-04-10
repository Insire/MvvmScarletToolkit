using Microsoft.Xaml.Behaviors;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace MvvmScarletToolkit.Wpf
{
    /// <summary>
    /// Behavior that enables scrolling a SelectedItem of a <see cref="ListBox"/> or <see cref="DataGrid"/> into the View
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
    public sealed class ScrollSelectionIntoView : Behavior<Selector>
    {
        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.SelectionChanged -= OnSelectionChanged;
            AssociatedObject.SelectionChanged += OnSelectionChanged;
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is DataGrid dataGrid)
            {
                if (dataGrid.SelectedItem is null)
                {
                    return;
                }

                var selectedItem = dataGrid.SelectedItem;

                dataGrid.Dispatcher.Invoke(ScrollIntoView);
                return;

                void ScrollIntoView()
                {
                    dataGrid.UpdateLayout();

                    dataGrid.ScrollIntoView(selectedItem);
                }
            }

            if (sender is ListBox listBox)
            {
                if (listBox.SelectedItem is null)
                {
                    return;
                }

                var selectedItem = listBox.SelectedItem;

                listBox.Dispatcher.Invoke(ScrollIntoView);

                void ScrollIntoView()
                {
                    listBox.UpdateLayout();

                    listBox.ScrollIntoView(selectedItem);
                }
            }
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            AssociatedObject.SelectionChanged -= OnSelectionChanged;
        }
    }
}
