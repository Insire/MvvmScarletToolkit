using Microsoft.Xaml.Behaviors;
using System.Collections;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace MvvmScarletToolkit
{
    /// <summary>
    /// Behavior enables binding <see cref="MultiSelector.SelectedItems"/> to an existing instance of <see cref="IList"/>
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
    //    <mvvm:MultiSelectionBehavior SelectedItems = "{Binding SelectedItems}" />
    // </ i:Interaction.Behaviors>
    public sealed class MultiSelectionBehavior : Behavior<MultiSelector>
    {
        public IList? SelectedItems
        {
            get { return (IList?)GetValue(SelectedItemsProperty); }
            set { SetValue(SelectedItemsProperty, value); }
        }

        /// <summary>Identifies the <see cref="SelectedItems"/> dependency property.</summary>
        public static readonly DependencyProperty SelectedItemsProperty = DependencyProperty.Register(
            nameof(SelectedItems)
            , typeof(IList)
            , typeof(MultiSelectionBehavior)
            , new UIPropertyMetadata(default(IList), OnSelectedItemsChanged));

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.SelectionChanged += OnSelectionChanged;

            if (SelectedItems != null)
            {
                if (AssociatedObject.SelectedItems.Count > 0)
                {
                    AssociatedObject.SelectedItems.Clear();
                }

                foreach (var item in SelectedItems)
                {
                    AssociatedObject.SelectedItems.Add(item);
                }
            }
        }

        protected override void OnDetaching()
        {
            AssociatedObject.SelectionChanged -= OnSelectionChanged;
            base.OnDetaching();
        }

        private static void OnSelectedItemsChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            if (o is not MultiSelectionBehavior behavior)
            {
                return;
            }

            if (behavior.AssociatedObject is null)
            {
                return;
            }

            if (e.OldValue is INotifyCollectionChanged oldValue)
            {
                oldValue.CollectionChanged -= behavior.SourceCollectionChanged;
            }

            if (e.NewValue is INotifyCollectionChanged newValue)
            {
                if (e.OldValue is not null) // skip setting the initial value from the UI(since thats an empty collection), as that will overwrite anything that has been set in the bound object
                {
                    if (behavior.AssociatedObject.SelectedItems.Count > 0)
                    {
                        behavior.AssociatedObject.SelectedItems.Clear();
                    }
                }

                foreach (var item in (IEnumerable)newValue)
                {
                    behavior.AssociatedObject.SelectedItems.Add(item);
                }

                newValue.CollectionChanged += behavior.SourceCollectionChanged;
            }
        }

        private bool _isUpdatingTarget;
        private bool _isUpdatingSource;

        private void SourceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (_isUpdatingSource)
            {
                return;
            }

            try
            {
                _isUpdatingTarget = true;

                if (e.OldItems is not null)
                {
                    foreach (var item in e.OldItems)
                    {
                        AssociatedObject.SelectedItems.Remove(item);
                    }
                }

                if (e.NewItems is not null)
                {
                    foreach (var item in e.NewItems)
                    {
                        AssociatedObject.SelectedItems.Add(item);
                    }
                }

                if (e.Action == NotifyCollectionChangedAction.Reset)
                {
                    AssociatedObject.SelectedItems.Clear();
                }
            }
            finally
            {
                _isUpdatingTarget = false;
            }
        }

        private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (_isUpdatingTarget)
            {
                return;
            }

            if (!ReferenceEquals(e.OriginalSource, sender))
            {
                return;
            }

            var selectedItems = SelectedItems;
            if (selectedItems is null)
            {
                return;
            }

            try
            {
                _isUpdatingSource = true;

                foreach (var item in e.RemovedItems)
                {
                    selectedItems.Remove(item);
                }

                foreach (var item in e.AddedItems)
                {
                    selectedItems.Add(item);
                }
            }
            finally
            {
                _isUpdatingSource = false;
            }
        }
    }
}
