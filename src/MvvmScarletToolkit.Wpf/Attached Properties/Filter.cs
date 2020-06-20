using System;
using System.ComponentModel;
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

        public static readonly DependencyProperty ViewByProperty = DependencyProperty.RegisterAttached(
            "ViewBy",
            typeof(Predicate<object>),
            typeof(Filter),
            new PropertyMetadata(default(Predicate<object>), OnViewByChanged));

        /// <summary>Helper for setting <see cref="ViewByProperty"/> on <paramref name="element"/>.</summary>
        /// <param name="element"><see cref="CollectionViewSource"/> to set <see cref="ViewByProperty"/> on.</param>
        /// <param name="value">ViewBy property value.</param>
        public static void SetViewBy(CollectionViewSource element, Predicate<object> value)
        {
            element.SetValue(ViewByProperty, value);
        }

        /// <summary>Helper for getting <see cref="ViewByProperty"/> from <paramref name="element"/>.</summary>
        /// <param name="element"><see cref="CollectionViewSource"/> to read <see cref="ViewByProperty"/> from.</param>
        /// <returns>ViewBy property value.</returns>
        [AttachedPropertyBrowsableForType(typeof(CollectionViewSource))]
        public static Predicate<object> GetViewBy(CollectionViewSource element)
        {
            return (Predicate<object>)element.GetValue(ViewByProperty);
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
            if (!(e.NewValue is Predicate<object> predicate))
            {
                return;
            }

            if (d is ItemsControl itemsControl && itemsControl.Items.CanFilter)
            {
                FilterItemsControl(itemsControl, predicate);
                return;
            }
        }

        private static void OnViewByChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(e.NewValue is Predicate<object> predicate))
            {
                return;
            }

            switch (d)
            {
                case CollectionViewSource source:

                    using (source.DeferRefresh())
                    {
                        source.Filter -= new FilterEventHandler(FilterAdapter);
                        source.Filter += new FilterEventHandler(FilterAdapter);
                    }
                    break;

                case ICollectionView view:
                    FilterCollectioNView(view, predicate);
                    break;
            }

            void FilterAdapter(object sender, FilterEventArgs e)
            {
                var source = sender as CollectionViewSource;
                if (source is null)
                {
                    return;
                }

                e.Accepted = predicate(e.Item);
            }
        }

        private static void FilterItemsControl(ItemsControl itemsControl, Predicate<object> predicate)
        {
            FilterCollectioNView(itemsControl.Items, predicate);
        }

        private static void FilterCollectioNView(ICollectionView view, Predicate<object> predicate)
        {
            using (view.DeferRefresh())
            {
                view.Filter = predicate;
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
