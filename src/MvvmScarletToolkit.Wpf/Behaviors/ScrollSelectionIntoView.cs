using Microsoft.Xaml.Behaviors;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace MvvmScarletToolkit.Wpf
{
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
            if (sender is ListBox listBox)
            {
                if (listBox.SelectedItem is null)
                {
                    return;
                }

                listBox.Dispatcher.Invoke(ScrollIntoView);
            }

            void ScrollIntoView()
            {
                listBox.UpdateLayout();

                if (listBox.SelectedItem == null)
                {
                    return;
                }

                listBox.ScrollIntoView(listBox.SelectedItem);
            }
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            AssociatedObject.SelectionChanged -= OnSelectionChanged;
        }
    }
}
