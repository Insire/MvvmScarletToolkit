using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using System.Collections.Specialized;

namespace MvvmScarletToolkit
{
    /// <summary>
    /// Realizes and recycles the containers of a panel that lays out more than a single item per row, derived classes only provide the layout.
    /// Like the VirtualizingStackPanel it measures to the size of all items and relies on an outer ScrollViewer, which it observes through the effective viewport.
    /// </summary>
    public abstract class VirtualizingItemsPanel : VirtualizingPanel
    {
        // items this far outside of the viewport stay realized, relative to its size, so that scrolling doesn't measure on every step
        private const double CacheLength = 0.5;

        // used until the first layout pass reported an effective viewport
        private const double FallbackViewportSize = 1000;

        private readonly Dictionary<int, Control> _realized = new();
        private readonly Dictionary<object, Stack<Control>> _recyclePool = new();
        private readonly Dictionary<Control, object> _recycleKeys = new();
        private readonly HashSet<Control> _itemsThatAreContainers = new();
        private readonly HashSet<int> _usedInMeasure = new();

        private Rect _viewport;
        private Rect _realizedArea;

        protected VirtualizingItemsPanel()
        {
            EffectiveViewportChanged += OnEffectiveViewportChanged;
        }

        /// <summary>
        /// Measures the items, calling <see cref="Realize"/> for the ones inside of <paramref name="viewport"/>, and returns the size of all items.
        /// </summary>
        protected abstract Size MeasureItems(Size availableSize, Rect viewport);

        /// <summary>
        /// The bounds of an item, as determined by the last call to <see cref="MeasureItems"/>
        /// </summary>
        protected abstract Rect GetItemBounds(int index);

        /// <summary>
        /// The neighbour of an item for keyboard navigation, the result does not have to be a valid index
        /// </summary>
        protected abstract int GetAdjacentIndex(int index, NavigationDirection direction);

        /// <summary>
        /// Returns the container for the item, which stays realized until a measure pass doesn't ask for it anymore
        /// </summary>
        protected Control Realize(int index)
        {
            _usedInMeasure.Add(index);

            if (_realized.TryGetValue(index, out var realized))
            {
                return realized;
            }

            var container = CreateOrRecycle(Items[index], index);
            _realized[index] = container;

            return container;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            if (ItemsControl is null)
            {
                return default;
            }

            var viewport = GetViewport(availableSize);
            _realizedArea = viewport.Inflate(new Thickness(viewport.Width * CacheLength, viewport.Height * CacheLength));
            _usedInMeasure.Clear();

            var size = MeasureItems(availableSize, _realizedArea);

            foreach (var (index, container) in _realized.ToArray())
            {
                // the focused container has to survive, keyboard navigation continues from it
                if (!_usedInMeasure.Contains(index) && !container.IsKeyboardFocusWithin)
                {
                    _realized.Remove(index);
                    Recycle(container);
                }
            }

            return size;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            foreach (var (index, container) in _realized)
            {
                container.Arrange(GetItemBounds(index));
            }

            return finalSize;
        }

        protected override void OnItemsChanged(IReadOnlyList<object?> items, NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(items, e);

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    ShiftIndices(e.NewStartingIndex, e.NewItems!.Count);
                    break;

                case NotifyCollectionChangedAction.Remove:
                    RecycleRange(e.OldStartingIndex, e.OldItems!.Count);
                    ShiftIndices(e.OldStartingIndex, -e.OldItems!.Count);
                    break;

                default:
                    RecycleRange(0, int.MaxValue);
                    break;
            }

            InvalidateMeasure();
        }

        protected override void OnItemsControlChanged(ItemsControl? oldValue)
        {
            base.OnItemsControlChanged(oldValue);

            foreach (var container in _realized.Values.Concat(_recyclePool.Values.SelectMany(p => p)).Concat(_itemsThatAreContainers).Distinct())
            {
                RemoveInternalChild(container);
            }

            _realized.Clear();
            _recyclePool.Clear();
            _recycleKeys.Clear();
            _itemsThatAreContainers.Clear();
        }

        protected override Control? ContainerFromIndex(int index)
        {
            return _realized.GetValueOrDefault(index);
        }

        protected override int IndexFromContainer(Control container)
        {
            foreach (var (index, realized) in _realized)
            {
                if (ReferenceEquals(realized, container))
                {
                    return index;
                }
            }

            return -1;
        }

        protected override IEnumerable<Control>? GetRealizedContainers()
        {
            return _realized.OrderBy(p => p.Key).Select(p => p.Value);
        }

        protected override Control? ScrollIntoView(int index)
        {
            if (index < 0 || index >= Items.Count)
            {
                return null;
            }

            this.BringIntoView(GetItemBounds(index));
            UpdateLayout();

            return ContainerFromIndex(index);
        }

        protected override IInputElement? GetControl(NavigationDirection direction, IInputElement? from, bool wrap)
        {
            var count = Items.Count;
            if (count == 0)
            {
                return null;
            }

            var current = from is Control control ? IndexFromContainer(control) : -1;
            var target = direction switch
            {
                NavigationDirection.First => 0,
                NavigationDirection.Last => count - 1,
                NavigationDirection.Next => current + 1,
                NavigationDirection.Previous => current - 1,
                _ when current == -1 => 0,
                _ => GetAdjacentIndex(current, direction),
            };

            if (wrap)
            {
                target = (target + count) % count;
            }

            return target >= 0 && target < count ? ScrollIntoView(target) : null;
        }

        private void OnEffectiveViewportChanged(object? sender, EffectiveViewportChangedEventArgs e)
        {
            var viewport = e.EffectiveViewport;
            if (viewport.Width <= 0 || viewport.Height <= 0)
            {
                return;
            }

            // the first measure pass guessed the viewport
            var isFirstViewport = _viewport.Width <= 0;

            _viewport = viewport;

            if (isFirstViewport || !_realizedArea.Contains(viewport))
            {
                InvalidateMeasure();
            }
        }

        private Rect GetViewport(Size availableSize)
        {
            if (_viewport.Width > 0 && _viewport.Height > 0)
            {
                return _viewport;
            }

            return new Rect(0, 0, GetFiniteOrFallback(availableSize.Width), GetFiniteOrFallback(availableSize.Height));
        }

        private static double GetFiniteOrFallback(double value)
        {
            return double.IsInfinity(value) ? FallbackViewportSize : value;
        }

        private Control CreateOrRecycle(object? item, int index)
        {
            var generator = ItemContainerGenerator!;

            if (!generator.NeedsContainer(item, index, out var recycleKey))
            {
                var itemContainer = (Control)item!;
                if (_itemsThatAreContainers.Add(itemContainer))
                {
                    generator.PrepareItemContainer(itemContainer, item, index);
                    AddInternalChild(itemContainer);
                    generator.ItemContainerPrepared(itemContainer, item, index);
                }
                else
                {
                    itemContainer.SetCurrentValue(IsVisibleProperty, true);
                }

                return itemContainer;
            }

            if (recycleKey is not null && _recyclePool.TryGetValue(recycleKey, out var pool) && pool.TryPop(out var recycled))
            {
                recycled.SetCurrentValue(IsVisibleProperty, true);
                generator.PrepareItemContainer(recycled, item, index);
                generator.ItemContainerPrepared(recycled, item, index);

                return recycled;
            }

            var container = generator.CreateContainer(item, index, recycleKey);
            if (recycleKey is not null)
            {
                _recycleKeys[container] = recycleKey;
            }

            generator.PrepareItemContainer(container, item, index);
            AddInternalChild(container);
            generator.ItemContainerPrepared(container, item, index);

            return container;
        }

        private void Recycle(Control container)
        {
            // items can't be separated from their container, those only get hidden
            if (_itemsThatAreContainers.Contains(container))
            {
                container.SetCurrentValue(IsVisibleProperty, false);
                return;
            }

            ItemContainerGenerator!.ClearItemContainer(container);

            if (!_recycleKeys.TryGetValue(container, out var recycleKey))
            {
                RemoveInternalChild(container);
                return;
            }

            if (!_recyclePool.TryGetValue(recycleKey, out var pool))
            {
                _recyclePool[recycleKey] = pool = new Stack<Control>();
            }

            container.SetCurrentValue(IsVisibleProperty, false);
            pool.Push(container);
        }

        private void RecycleRange(int start, int count)
        {
            foreach (var (index, container) in _realized.ToArray())
            {
                if (index >= start && index - start < count)
                {
                    _realized.Remove(index);

                    if (_itemsThatAreContainers.Remove(container))
                    {
                        RemoveInternalChild(container);
                    }
                    else
                    {
                        Recycle(container);
                    }
                }
            }
        }

        private void ShiftIndices(int start, int delta)
        {
            foreach (var (index, container) in _realized.Where(p => p.Key >= start).OrderBy(p => delta > 0 ? -p.Key : p.Key).ToArray())
            {
                _realized.Remove(index);
                _realized[index + delta] = container;
                ItemContainerGenerator!.ItemContainerIndexChanged(container, index, index + delta);
            }
        }
    }
}
