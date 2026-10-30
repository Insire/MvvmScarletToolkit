using Microsoft.Xaml.Behaviors;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace MvvmScarletToolkit.Wpf
{
    /// <summary>
    /// Behavior that connects a <see cref="DataGridFilterViewModel"/> to a <see cref="DataGrid"/>, and publishes the filter of each column through <see cref="ColumnFilterProperty"/>
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
    //    <mvvm:DataGridFilterBehavior Filter="{Binding Filter}" />
    // </ i:Interaction.Behaviors>
    public sealed class DataGridFilterBehavior : Behavior<DataGrid>
    {
        public DataGridFilterViewModel? Filter
        {
            get { return (DataGridFilterViewModel?)GetValue(FilterProperty); }
            set { SetValue(FilterProperty, value); }
        }

        public static readonly DependencyProperty FilterProperty = DependencyProperty.Register(
            nameof(Filter),
            typeof(DataGridFilterViewModel),
            typeof(DataGridFilterBehavior),
            new PropertyMetadata(null, OnFilterChanged));

        /// <summary>
        /// The filter of a column. Column headers bind to this.
        /// </summary>
        public static readonly DependencyProperty ColumnFilterProperty = DependencyProperty.RegisterAttached(
            "ColumnFilter",
            typeof(ColumnFilterViewModel),
            typeof(DataGridFilterBehavior),
            new PropertyMetadata(null));

        public static void SetColumnFilter(DependencyObject element, ColumnFilterViewModel? value)
        {
            element.SetValue(ColumnFilterProperty, value);
        }

        [AttachedPropertyBrowsableForType(typeof(DataGridColumn))]
        public static ColumnFilterViewModel? GetColumnFilter(DependencyObject element)
        {
            return (ColumnFilterViewModel?)element.GetValue(ColumnFilterProperty);
        }

        private static void OnFilterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DataGridFilterBehavior { AssociatedObject: not null } behavior)
            {
                behavior.Connect();
            }
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.Columns.CollectionChanged += OnColumnsChanged;
            Connect();
        }

        protected override void OnDetaching()
        {
            AssociatedObject.Columns.CollectionChanged -= OnColumnsChanged;
            Disconnect();

            base.OnDetaching();
        }

        private void OnColumnsChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateColumns();
        }

        private void Connect()
        {
            Disconnect();

            if (Filter is null)
            {
                return;
            }

            UpdateColumns();

            MvvmScarletToolkit.Filter.SetBy(AssociatedObject, Filter.Predicate);
            BindingOperations.SetBinding(AssociatedObject, MvvmScarletToolkit.Filter.RefreshWhenChangedProperty, new Binding(nameof(DataGridFilterViewModel.Version))
            {
                Source = Filter,
                Mode = BindingMode.OneWay,
            });
        }

        private void Disconnect()
        {
            BindingOperations.ClearBinding(AssociatedObject, MvvmScarletToolkit.Filter.RefreshWhenChangedProperty);
            AssociatedObject.ClearValue(MvvmScarletToolkit.Filter.ByProperty);

            foreach (var column in AssociatedObject.Columns)
            {
                column.ClearValue(ColumnFilterProperty);
            }
        }

        private void UpdateColumns()
        {
            if (Filter is null)
            {
                return;
            }

            foreach (var column in AssociatedObject.Columns)
            {
                var path = column.GetPropertyPath();

                SetColumnFilter(column, path is null ? null : Filter.GetFilter(path));
            }
        }
    }
}
