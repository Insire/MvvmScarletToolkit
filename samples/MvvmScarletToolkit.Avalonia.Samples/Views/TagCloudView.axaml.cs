using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Layout;

namespace MvvmScarletToolkit.Avalonia.Samples.Views
{
    public partial class TagCloudView : UserControl
    {
        public TagCloudView()
        {
            InitializeComponent();

            Tags.LayoutUpdated += (_, _) => Statistics.Text = $"{Tags.ItemCount:N0} tags, {Tags.GetRealizedContainers().Count():N0} of them realized";
        }

        private void Columns_IsCheckedChanged(object? sender, RoutedEventArgs e)
        {
            if (sender is not ToggleButton toggle || Tags.ItemsPanelRoot is not VirtualizingWrapPanel panel)
            {
                return;
            }

            // items have to wrap in the direction that doesn't scroll
            var isVertical = toggle.IsChecked == true;

            panel.Orientation = isVertical ? Orientation.Vertical : Orientation.Horizontal;
            ScrollViewer.SetHorizontalScrollBarVisibility(Tags, isVertical ? ScrollBarVisibility.Auto : ScrollBarVisibility.Disabled);
            ScrollViewer.SetVerticalScrollBarVisibility(Tags, isVertical ? ScrollBarVisibility.Disabled : ScrollBarVisibility.Auto);
        }
    }
}
