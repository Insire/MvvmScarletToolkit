using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace MvvmScarletToolkit
{
    // source: https://stackoverflow.com/questions/15358113/wpf-filter-a-listbox/39438710#39438710
    public static class Filter
    {
        public static readonly DependencyProperty ByProperty = DependencyProperty.RegisterAttached(
            "By",
            typeof(Predicate<object>),
            typeof(Filter),
            new PropertyMetadata(default(Predicate<object>), OnByChanged));

        /// <summary>Helper for setting <see cref="ByProperty"/> on <paramref name="element"/>.</summary>
        /// <param name="element"><see cref="ItemsControl"/> to set <see cref="ByProperty"/> on.</param>
        /// <param name="value">By property value.</param>
        public static void SetBy(ItemsControl element, Predicate<object> value)
        {
            element.SetValue(ByProperty, value);
        }

        /// <summary>Helper for getting <see cref="ByProperty"/> from <paramref name="element"/>.</summary>
        /// <param name="element"><see cref="ItemsControl"/> to read <see cref="ByProperty"/> from.</param>
        /// <returns>By property value.</returns>
        [AttachedPropertyBrowsableForType(typeof(ItemsControl))]
        public static Predicate<object> GetBy(ItemsControl element)
        {
            return (Predicate<object>)element.GetValue(ByProperty);
        }

        public static readonly DependencyProperty RefreshWhenChangedProperty = DependencyProperty.RegisterAttached(
            "RefreshWhenChanged",
            typeof(object),
            typeof(Filter),
            new PropertyMetadata(default, OnRefreshWhenChangedChanged));

        /// <summary>Helper for setting <see cref="RefreshWhenChangedProperty"/> on <paramref name="element"/>.</summary>
        /// <param name="element"><see cref="ItemsControl"/> to set <see cref="RefreshWhenChangedProperty"/> on.</param>
        /// <param name="value">RefreshWhenChanged property value.</param>
        public static void SetRefreshWhenChanged(ItemsControl element, object value)
        {
            element.SetValue(RefreshWhenChangedProperty, value);
        }

        /// <summary>Helper for getting <see cref="RefreshWhenChangedProperty"/> from <paramref name="element"/>.</summary>
        /// <param name="element"><see cref="ItemsControl"/> to read <see cref="RefreshWhenChangedProperty"/> from.</param>
        /// <returns>RefreshWhenChanged property value.</returns>
        [AttachedPropertyBrowsableForType(typeof(ItemsControl))]
        public static object GetRefreshWhenChanged(ItemsControl element)
        {
            return element.GetValue(RefreshWhenChangedProperty);
        }

        private static void OnByChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is ItemsControl itemsControl) || !itemsControl.Items.CanFilter || !(e.NewValue is Predicate<object> predicate))
            {
                return;
            }

            using (itemsControl.Items.DeferRefresh())
            {
                itemsControl.Items.Filter = predicate;
            }
        }

        private static void OnRefreshWhenChangedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is ItemsControl itemsControl) || !itemsControl.Items.CanFilter)
            {
                return;
            }

            // we dont care what property was bound to this,
            // we only care that it changed. When it changes we refresh the view to force updating the filter
            var view = (CollectionView)CollectionViewSource.GetDefaultView(itemsControl.ItemsSource);

            view.Refresh();
        }
    }
}
