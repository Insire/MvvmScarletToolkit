using Avalonia;
using Avalonia.Input;
using Avalonia.Layout;

namespace MvvmScarletToolkit
{
    /// <summary>
    /// Lays out items next to each other and wraps them into the next row or column, like a WrapPanel.
    /// Items may differ in size, unless <see cref="ItemWidth"/> and <see cref="ItemHeight"/> set a fixed one.
    /// </summary>
    /// <remarks>
    /// <code>xmlns:mvvm="http://SoftThorn.MvvmScarletToolkit.com/winfx/xaml/shared"</code>
    /// The sizes of items that never got realized are estimated from the ones that did, so the scrollbar and
    /// the items after the estimated ones can shift a little while scrolling. Set a fixed size to avoid that.
    /// The available size has to be limited in the direction items wrap in, e.g. by disabling the horizontal scrollbar for the default orientation.
    /// </remarks>
    public sealed class VirtualizingWrapPanel : VirtualizingItemsPanel
    {
        public static readonly StyledProperty<Orientation> OrientationProperty = AvaloniaProperty.Register<VirtualizingWrapPanel, Orientation>(nameof(Orientation), Orientation.Horizontal);

        public static readonly StyledProperty<double> ItemWidthProperty = AvaloniaProperty.Register<VirtualizingWrapPanel, double>(nameof(ItemWidth), double.NaN);

        public static readonly StyledProperty<double> ItemHeightProperty = AvaloniaProperty.Register<VirtualizingWrapPanel, double>(nameof(ItemHeight), double.NaN);

        // by item, so that sizes survive inserting, removing and filtering items
        private readonly Dictionary<object, Size> _measuredSizes = new();

        private Rect[] _bounds = [];
        private Size _sizeOfMeasuredItems;

        public Orientation Orientation
        {
            get { return GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        public double ItemWidth
        {
            get { return GetValue(ItemWidthProperty); }
            set { SetValue(ItemWidthProperty, value); }
        }

        public double ItemHeight
        {
            get { return GetValue(ItemHeightProperty); }
            set { SetValue(ItemHeightProperty, value); }
        }

        static VirtualizingWrapPanel()
        {
            AffectsMeasure<VirtualizingWrapPanel>(OrientationProperty, ItemWidthProperty, ItemHeightProperty);
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == ItemWidthProperty || change.Property == ItemHeightProperty)
            {
                _measuredSizes.Clear();
                _sizeOfMeasuredItems = default;
            }
        }

        protected override Size MeasureItems(Size availableSize, Rect viewport)
        {
            var hasEstimate = _measuredSizes.Count > 0;
            var size = LayoutItems(availableSize, viewport);

            // without an estimate, all items that were not realized counted as empty
            return hasEstimate ? size : LayoutItems(availableSize, viewport);
        }

        // U is the direction items are placed in, V the one sections are stacked in.
        // Every pass walks all items, positions depend on all previous ones and this only costs some additions per item
        private Size LayoutItems(Size availableSize, Rect viewport)
        {
            var isHorizontal = Orientation == Orientation.Horizontal;
            var count = Items.Count;

            if (_bounds.Length != count)
            {
                _bounds = new Rect[count];
            }

            var context = CreateContext(availableSize, viewport, isHorizontal);

            var u = 0d;
            var v = 0d;
            var sectionSize = 0d;
            var largestU = 0d;
            var sectionStart = 0;

            for (var index = 0; index < count; index++)
            {
                var item = Items[index];
                var size = GetKnownSize(item, context);
                var wraps = context.Wraps(u, size);

                if (context.IsInViewport(v, sectionSize, size, wraps))
                {
                    size = MeasureRealized(index, item, context, size);
                    wraps = context.Wraps(u, size);
                }

                if (wraps)
                {
                    FinishSection(sectionStart, index, sectionSize, isHorizontal);
                    v += sectionSize;
                    u = 0;
                    sectionSize = 0;
                    sectionStart = index;
                }

                _bounds[index] = isHorizontal
                    ? new Rect(u, v, size.Width, size.Height)
                    : new Rect(v, u, size.Width, size.Height);

                u += GetU(size, isHorizontal);
                largestU = Math.Max(largestU, u);
                sectionSize = Math.Max(sectionSize, GetV(size, isHorizontal));
            }

            FinishSection(sectionStart, count, sectionSize, isHorizontal);

            var totalU = double.IsInfinity(GetU(availableSize, isHorizontal)) ? largestU : context.Limit;
            var totalV = v + sectionSize;

            return isHorizontal ? new Size(totalU, totalV) : new Size(totalV, totalU);
        }

        private LayoutContext CreateContext(Size availableSize, Rect viewport, bool isHorizontal)
        {
            var limit = GetU(availableSize, isHorizontal);
            if (double.IsInfinity(limit))
            {
                limit = GetU(viewport.Size, isHorizontal);
            }

            var slot = new Size(
                double.IsNaN(ItemWidth) ? double.PositiveInfinity : ItemWidth,
                double.IsNaN(ItemHeight) ? double.PositiveInfinity : ItemHeight);

            return new LayoutContext(
                isHorizontal,
                limit,
                First: isHorizontal ? viewport.Top : viewport.Left,
                Last: isHorizontal ? viewport.Bottom : viewport.Right,
                slot,
                IsFixedSize: !double.IsNaN(ItemWidth) && !double.IsNaN(ItemHeight),
                Estimate: GetEstimatedSize());
        }

        /// <summary>
        /// The fixed size, the last measured size, or an estimate for items that never got realized
        /// </summary>
        private Size GetKnownSize(object? item, LayoutContext context)
        {
            if (context.IsFixedSize)
            {
                return context.Slot;
            }

            return item is not null && _measuredSizes.TryGetValue(item, out var known)
                ? known
                : context.Estimate;
        }

        private Size MeasureRealized(int index, object? item, LayoutContext context, Size size)
        {
            var container = Realize(index);
            container.Measure(context.Slot);

            if (context.IsFixedSize)
            {
                return size;
            }

            Remember(item, container.DesiredSize);
            return container.DesiredSize;
        }

        /// <summary>
        /// Settings of one layout pass. U is the direction items are placed in, V the one sections are stacked in.
        /// </summary>
        private readonly record struct LayoutContext(bool IsHorizontal, double Limit, double First, double Last, Size Slot, bool IsFixedSize, Size Estimate)
        {
            public bool Wraps(double u, Size size)
            {
                return u > 0 && u + GetU(size, IsHorizontal) > Limit;
            }

            /// <summary>
            /// Whether an item of <paramref name="size"/> overlaps the viewport, when placed in the current section or at the start of the next one
            /// </summary>
            public bool IsInViewport(double v, double sectionSize, Size size, bool wraps)
            {
                var position = wraps ? v + sectionSize : v;

                return position <= Last
                    && position + Math.Max(wraps ? 0 : sectionSize, GetV(size, IsHorizontal)) >= First;
            }
        }

        protected override Rect GetItemBounds(int index)
        {
            return index < _bounds.Length ? _bounds[index] : default;
        }

        protected override int GetAdjacentIndex(int index, NavigationDirection direction)
        {
            var isHorizontal = Orientation == Orientation.Horizontal;

            return direction switch
            {
                NavigationDirection.Left => isHorizontal ? GetIndexInSection(index, -1) : GetIndexInNextSection(index, -1),
                NavigationDirection.Right => isHorizontal ? GetIndexInSection(index, 1) : GetIndexInNextSection(index, 1),
                NavigationDirection.Up => isHorizontal ? GetIndexInNextSection(index, -1) : GetIndexInSection(index, -1),
                NavigationDirection.Down => isHorizontal ? GetIndexInNextSection(index, 1) : GetIndexInSection(index, 1),
                _ => -1,
            };
        }

        private int GetIndexInSection(int index, int step)
        {
            var target = index + step;

            return target >= 0 && target < _bounds.Length && IsSameSection(index, target) ? target : -1;
        }

        // the item of the adjacent section that is closest to the current one
        private int GetIndexInNextSection(int index, int step)
        {
            var isHorizontal = Orientation == Orientation.Horizontal;
            var center = GetCenter(index, isHorizontal);

            var candidate = index;
            while (candidate >= 0 && candidate < _bounds.Length && IsSameSection(index, candidate))
            {
                candidate += step;
            }

            var best = -1;
            var bestDistance = double.MaxValue;

            for (var other = candidate; other >= 0 && other < _bounds.Length && IsSameSection(candidate, other); other += step)
            {
                var distance = Math.Abs(GetCenter(other, isHorizontal) - center);
                if (distance < bestDistance)
                {
                    best = other;
                    bestDistance = distance;
                }
            }

            return best;
        }

        private bool IsSameSection(int first, int second)
        {
            return Orientation == Orientation.Horizontal
                ? _bounds[first].Y.Equals(_bounds[second].Y)
                : _bounds[first].X.Equals(_bounds[second].X);
        }

        private double GetCenter(int index, bool isHorizontal)
        {
            return isHorizontal ? _bounds[index].Center.X : _bounds[index].Center.Y;
        }

        // items stretch across their section, like in a WrapPanel
        private void FinishSection(int start, int end, double sectionSize, bool isHorizontal)
        {
            for (var index = start; index < end; index++)
            {
                _bounds[index] = isHorizontal
                    ? _bounds[index].WithHeight(sectionSize)
                    : _bounds[index].WithWidth(sectionSize);
            }
        }

        private void Remember(object? item, Size size)
        {
            if (item is null)
            {
                return;
            }

            if (_measuredSizes.TryGetValue(item, out var previous))
            {
                _sizeOfMeasuredItems -= previous;
            }

            _measuredSizes[item] = size;
            _sizeOfMeasuredItems += size;
        }

        private Size GetEstimatedSize()
        {
            var size = new Size(double.IsNaN(ItemWidth) ? 0 : ItemWidth, double.IsNaN(ItemHeight) ? 0 : ItemHeight);

            if (_measuredSizes.Count == 0)
            {
                return size;
            }

            return new Size(
                double.IsNaN(ItemWidth) ? _sizeOfMeasuredItems.Width / _measuredSizes.Count : ItemWidth,
                double.IsNaN(ItemHeight) ? _sizeOfMeasuredItems.Height / _measuredSizes.Count : ItemHeight);
        }

        private static double GetU(Size size, bool isHorizontal)
        {
            return isHorizontal ? size.Width : size.Height;
        }

        private static double GetV(Size size, bool isHorizontal)
        {
            return isHorizontal ? size.Height : size.Width;
        }
    }
}
