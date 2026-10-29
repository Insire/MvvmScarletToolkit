using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace MvvmScarletToolkit
{
    /// <summary>
    /// Attached properties that enable binding <see cref="ICollectionView.Filter"/> and calling <see cref="ICollectionView.Refresh"/> when a property changes with XAML only
    /// </summary>
    /// <remarks>
    /// required namespaces:
    /// <list type="bullet">
    /// <item>
    /// <description>xmlns:mvvm="http://SoftThorn.MvvmScarletToolkit.com/winfx/xaml/shared"</description>
    /// </item>
    /// </list>
    /// </remarks>
    // original idea from: https://stackoverflow.com/questions/15358113/wpf-filter-a-listbox/39438710#39438710
    public static class Filter
    {
        /// <summary>
        /// the bindable predicate thats executed for filtering a <see cref="ICollectionView"/>
        /// </summary>
        /// <remarks>
        /// can be set on:
        /// <list type="bullet">
        /// <item>
        /// <description><see cref="CollectionViewSource"/> and sub types</description>
        /// </item>
        /// <item>
        /// <description><see cref="ItemsControl"/> and sub types</description>
        /// </item>
        /// </list>
        /// </remarks>
        public static readonly DependencyProperty ByProperty = DependencyProperty.RegisterAttached(
            "By",
            typeof(Predicate<object>),
            typeof(Filter),
            new PropertyMetadata(null, OnByChanged));

        /// <summary>Helper for setting <see cref="ByProperty"/> on <paramref name="element"/>.</summary>
        /// <param name="element"><see cref="DependencyObject"/> to set <see cref="ByProperty"/> on.</param>
        /// <param name="value">By property value.</param>
        public static void SetBy(DependencyObject element, Predicate<object> value)
        {
            element.SetValue(ByProperty, value);
        }

        /// <summary>Helper for getting <see cref="ByProperty"/> from <paramref name="element"/>.</summary>
        /// <param name="element"><see cref="DependencyObject"/> to read <see cref="ByProperty"/> from.</param>
        /// <returns>By property value.</returns>
        [AttachedPropertyBrowsableForType(typeof(DependencyObject))]
        public static Predicate<object> GetBy(DependencyObject element)
        {
            return (Predicate<object>)element.GetValue(ByProperty);
        }

        /// <summary>
        /// the property that we listen for changes on, so that we can refresh its <see cref="ICollectionView"/>
        /// </summary>
        /// <remarks>
        /// can be set on:
        /// <list type="bullet">
        /// <item>
        /// <description><see cref="CollectionViewSource"/> and sub types</description>
        /// </item>
        /// <item>
        /// <description><see cref="ItemsControl"/> and sub types</description>
        /// </item>
        /// </list>
        /// </remarks>
        public static readonly DependencyProperty RefreshWhenChangedProperty = DependencyProperty.RegisterAttached(
            "RefreshWhenChanged",
            typeof(object),
            typeof(Filter),
            new PropertyMetadata(null, OnRefreshWhenChangedChanged));

        /// <summary>Helper for setting <see cref="RefreshWhenChangedProperty"/> on <paramref name="element"/>.</summary>
        /// <param name="element"><see cref="ItemsControl"/> to set <see cref="RefreshWhenChangedProperty"/> on.</param>
        /// <param name="value">RefreshWhenChanged property value.</param>
        public static void SetRefreshWhenChanged(DependencyObject element, object value)
        {
            element.SetValue(RefreshWhenChangedProperty, value);
        }

        /// <summary>Helper for getting <see cref="RefreshWhenChangedProperty"/> from <paramref name="element"/>.</summary>
        /// <param name="element"><see cref="ItemsControl"/> to read <see cref="RefreshWhenChangedProperty"/> from.</param>
        /// <returns>RefreshWhenChanged property value.</returns>
        [AttachedPropertyBrowsableForType(typeof(DependencyObject))]
        public static object GetRefreshWhenChanged(DependencyObject element)
        {
            return element.GetValue(RefreshWhenChangedProperty);
        }

        private static readonly DependencyPropertyDescriptor _viewDescriptor = DependencyPropertyDescriptor.FromProperty(CollectionViewSource.ViewProperty, typeof(CollectionViewSource));

        private static void OnByChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var oldPredicate = e.OldValue as Predicate<object>;
            var predicate = e.NewValue as Predicate<object>;

            switch (d)
            {
                case ItemsControl { Items.CanFilter: true } itemsControl:
                    ApplyFilter(itemsControl.Items, oldPredicate, predicate);
                    return;

                case CollectionViewSource collectionViewSource:
                    // the view may not exist yet or get replaced later, e.g. when Source changes.
                    // a static handler, a local function capturing variables would be a new delegate on every call and could never be removed
                    _viewDescriptor.RemoveValueChanged(collectionViewSource, OnViewChanged);
                    if (predicate is not null)
                    {
                        _viewDescriptor.AddValueChanged(collectionViewSource, OnViewChanged);
                    }

                    if (collectionViewSource.View is not null)
                    {
                        ApplyFilter(collectionViewSource.View, oldPredicate, predicate);
                    }

                    return;
            }
        }

        private static void OnViewChanged(object? sender, EventArgs args)
        {
            if (sender is CollectionViewSource { View: not null } collectionViewSource)
            {
                ApplyFilter(collectionViewSource.View, null, collectionViewSource.GetValue(ByProperty) as Predicate<object>);
            }
        }

        private static void ApplyFilter(ICollectionView view, Predicate<object>? oldPredicate, Predicate<object>? predicate)
        {
            if (!view.CanFilter)
            {
                return;
            }

            if (predicate is null)
            {
                // only clear a filter that we set
                if (oldPredicate is not null && Equals(view.Filter, oldPredicate))
                {
                    view.Filter = null;
                }

                return;
            }

            // replace instead of combining, a multicast predicate only returns the result of its last delegate
            view.Filter = predicate;
        }

        private static void OnRefreshWhenChangedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            switch (d)
            {
                case CollectionViewSource { View.CanFilter: true } collectionViewSource:
                    collectionViewSource.View.Refresh();
                    break;

                case ItemsControl { Items.CanFilter: true } itemsControl:
                {
                    // we dont care what property was bound to this,
                    // we only care that it changed. When it changes we refresh the view to force updating the filter
                    var view = CollectionViewSource.GetDefaultView(itemsControl.ItemsSource) as CollectionView;

                    view?.Refresh();
                    break;
                }
            }
        }
    }
}
