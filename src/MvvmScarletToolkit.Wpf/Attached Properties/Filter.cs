using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace MvvmScarletToolkit
{
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
            new PropertyMetadata(default(Predicate<object>), OnByChanged));

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
            new PropertyMetadata(default, OnRefreshWhenChangedChanged));

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

        private static void OnByChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(e.NewValue is Predicate<object> predicate))
            {
                return;
            }

            if (d is ItemsControl itemsControl && itemsControl.Items.CanFilter)
            {
                AttachFilterToCollectionView(itemsControl.Items, predicate);
                return;
            }

            if (d is CollectionViewSource collectionViewSource)
            {
                if (collectionViewSource.View is null)
                {
                    var propertyDescriptor = DependencyPropertyDescriptor.FromProperty(CollectionViewSource.ViewProperty, typeof(CollectionViewSource));
                    propertyDescriptor?.RemoveValueChanged(collectionViewSource, OnViewChanged);
                    propertyDescriptor?.AddValueChanged(collectionViewSource, OnViewChanged);

                    void OnViewChanged(object sender, EventArgs args)
                    {
                        if (collectionViewSource.View is null)
                        {
                            return;
                        }

                        AttachFilterToCollectionView(collectionViewSource.View, predicate);
                    }
                }
                else
                {
                    AttachFilterToCollectionView(collectionViewSource.View, predicate);
                }
            }
        }

        private static void AttachFilterToCollectionView(ICollectionView view, Predicate<object> predicate)
        {
            using (view.DeferRefresh())
            {
                if (view.Filter is null)
                {
                    view.Filter = predicate;
                }
                else
                {
                    view.Filter -= predicate;
                    view.Filter += predicate;
                }
            }
        }

        private static void OnRefreshWhenChangedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CollectionViewSource collectionViewSource && collectionViewSource.View?.CanFilter == true)
            {
                collectionViewSource.View.Refresh();
            }

            if (d is ItemsControl itemsControl && itemsControl.Items?.CanFilter == true)
            {
                // we dont care what property was bound to this,
                // we only care that it changed. When it changes we refresh the view to force updating the filter
                var view = CollectionViewSource.GetDefaultView(itemsControl.ItemsSource) as CollectionView;

                view?.Refresh();
            }
        }
    }
}
