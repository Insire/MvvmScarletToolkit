using System.Windows.Controls;
using System.Windows.Media;

namespace MvvmScarletToolkit.Wpf.Samples.Controls
{
    public partial class TagCloud : UserControl
    {
        public TagCloud()
        {
            InitializeComponent();
        }

        private void Tags_LayoutUpdated(object? sender, EventArgs e)
        {
            if (FindPanel(Tags) is { } panel)
            {
                Statistics.Text = $"{Tags.Items.Count:N0} tags, {panel.Children.Count:N0} of them realized";
            }
        }

        private static VirtualizingWrapPanel? FindPanel(Visual parent)
        {
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                if (VisualTreeHelper.GetChild(parent, i) is not Visual child)
                {
                    continue;
                }

                if ((child as VirtualizingWrapPanel ?? FindPanel(child)) is { } panel)
                {
                    return panel;
                }
            }

            return null;
        }
    }
}
