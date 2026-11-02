using Avalonia.Controls;

namespace MvvmScarletToolkit.Avalonia.Samples.Behaviors
{
    /// <summary>
    /// Notifies about items entering and leaving the viewport of a virtualizing list.
    /// Avalonia recycles containers, so their item templates never get unloaded while scrolling, unlike Standard virtualization in WPF.
    /// </summary>
    public sealed class ContainerLifecycle<TItem>
        where TItem : class
    {
        // the container may have lost its DataContext by the time it is cleared
        private readonly Dictionary<Control, TItem> _items = new();
        private readonly Action<TItem> _prepared;
        private readonly Action<TItem> _cleared;

        public ContainerLifecycle(ItemsControl itemsControl, Action<TItem> prepared, Action<TItem> cleared)
        {
            _prepared = prepared;
            _cleared = cleared;

            itemsControl.ContainerPrepared += OnContainerPrepared;
            itemsControl.ContainerClearing += OnContainerClearing;
        }

        private void OnContainerPrepared(object? sender, ContainerPreparedEventArgs e)
        {
            if (e.Container.DataContext is TItem item)
            {
                _items[e.Container] = item;
                _prepared(item);
            }
        }

        private void OnContainerClearing(object? sender, ContainerClearingEventArgs e)
        {
            if (_items.Remove(e.Container, out var item))
            {
                _cleared(item);
            }
        }
    }
}
