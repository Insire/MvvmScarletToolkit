using Avalonia;
using Avalonia.Input;
using Avalonia.Layout;

namespace MvvmScarletToolkit
{
    /// <summary>
    /// Lays out items as square tiles of <see cref="ChildSize"/>, in as many columns as fit the available width.
    /// </summary>
    /// <remarks>
    /// <code>xmlns:mvvm="http://SoftThorn.MvvmScarletToolkit.com/winfx/xaml/shared"</code>
    /// </remarks>
    public sealed class VirtualizingTilePanel : VirtualizingItemsPanel
    {
        public static readonly StyledProperty<double> ChildSizeProperty = AvaloniaProperty.Register<VirtualizingTilePanel, double>(nameof(ChildSize), 200d);

        private int _columns = 1;

        public double ChildSize
        {
            get { return GetValue(ChildSizeProperty); }
            set { SetValue(ChildSizeProperty, value); }
        }

        static VirtualizingTilePanel()
        {
            AffectsMeasure<VirtualizingTilePanel>(ChildSizeProperty);
        }

        protected override Size MeasureItems(Size availableSize, Rect viewport)
        {
            var childSize = ChildSize;
            var count = Items.Count;
            var width = double.IsInfinity(availableSize.Width) ? viewport.Width : availableSize.Width;

            _columns = Math.Max(1, (int)Math.Floor(width / childSize));

            var rows = (int)Math.Ceiling((double)count / _columns);
            var firstRow = Math.Max(0, (int)Math.Floor(viewport.Top / childSize));
            var lastRow = Math.Min(rows - 1, (int)Math.Ceiling(viewport.Bottom / childSize) - 1);

            var first = firstRow * _columns;
            var last = Math.Min(count - 1, ((lastRow + 1) * _columns) - 1);

            for (var index = first; index <= last; index++)
            {
                Realize(index).Measure(new Size(childSize, childSize));
            }

            return new Size(double.IsInfinity(availableSize.Width) ? _columns * childSize : availableSize.Width, rows * childSize);
        }

        protected override Rect GetItemBounds(int index)
        {
            var childSize = ChildSize;

            return new Rect((index % _columns) * childSize, (index / _columns) * childSize, childSize, childSize);
        }

        protected override int GetAdjacentIndex(int index, NavigationDirection direction)
        {
            return direction switch
            {
                NavigationDirection.Left => index % _columns == 0 ? -1 : index - 1,
                NavigationDirection.Right => (index + 1) % _columns == 0 ? -1 : index + 1,
                NavigationDirection.Up => index - _columns,
                NavigationDirection.Down => index + _columns,
                _ => -1,
            };
        }
    }
}
