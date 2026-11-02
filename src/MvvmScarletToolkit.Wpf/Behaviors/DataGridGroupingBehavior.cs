using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace MvvmScarletToolkit.Wpf
{
    /// <summary>
    /// Behavior that groups a <see cref="DataGrid"/> by dragging a column header onto <see cref="DropTarget"/>.
    /// The column is taken from <see cref="DataGridColumn.SortMemberPath"/>, so columns without one can't be dragged.
    /// </summary>
    /// <remarks>
    /// <para>A drag starts when the mouse leaves the header row, moving along the headers still reorders columns.</para>
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
    //    <mvvm:DataGridGroupingBehavior DropTarget="{Binding ElementName=GroupPanel}" Grouping="{Binding Groups}" />
    // </ i:Interaction.Behaviors>
    public sealed class DataGridGroupingBehavior : Behavior<DataGrid>
    {
        private static readonly string _dataFormat = typeof(DataGridGroupingBehavior).FullName!;

        private DataGridColumnHeader? _header;

        public GroupingViewModel? Grouping
        {
            get { return (GroupingViewModel?)GetValue(GroupingProperty); }
            set { SetValue(GroupingProperty, value); }
        }

        public static readonly DependencyProperty GroupingProperty = DependencyProperty.Register(
            nameof(Grouping),
            typeof(GroupingViewModel),
            typeof(DataGridGroupingBehavior),
            new PropertyMetadata(null));

        public UIElement? DropTarget
        {
            get { return (UIElement?)GetValue(DropTargetProperty); }
            set { SetValue(DropTargetProperty, value); }
        }

        public static readonly DependencyProperty DropTargetProperty = DependencyProperty.Register(
            nameof(DropTarget),
            typeof(UIElement),
            typeof(DataGridGroupingBehavior),
            new PropertyMetadata(null, OnDropTargetChanged));

        private static void OnDropTargetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DataGridGroupingBehavior behavior)
            {
                return;
            }

            if (e.OldValue is UIElement oldTarget)
            {
                behavior.Disconnect(oldTarget);
            }

            if (e.NewValue is UIElement newTarget)
            {
                behavior.Connect(newTarget);
            }
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.PreviewMouseLeftButtonDown += OnPreviewMouseLeftButtonDown;
            AssociatedObject.PreviewMouseLeftButtonUp += OnPreviewMouseLeftButtonUp;
            AssociatedObject.PreviewMouseMove += OnPreviewMouseMove;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.PreviewMouseLeftButtonDown -= OnPreviewMouseLeftButtonDown;
            AssociatedObject.PreviewMouseLeftButtonUp -= OnPreviewMouseLeftButtonUp;
            AssociatedObject.PreviewMouseMove -= OnPreviewMouseMove;

            if (DropTarget is not null)
            {
                Disconnect(DropTarget);
            }

            base.OnDetaching();
        }

        private void Connect(UIElement target)
        {
            target.AllowDrop = true;
            target.DragOver += OnDragOver;
            target.Drop += OnDrop;
        }

        private void Disconnect(UIElement target)
        {
            target.DragOver -= OnDragOver;
            target.Drop -= OnDrop;
        }

        private void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var source = e.OriginalSource as DependencyObject;
            var header = source?.FindParent<DataGridColumnHeader>();

            _header = header?.Column is { SortMemberPath.Length: > 0 } && source?.FindParent<DataGridColumnFilter>() is null ? header : null;
        }

        private void OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _header = null;
        }

        private void OnPreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (_header is null || e.LeftButton != MouseButtonState.Pressed || Grouping is null || DropTarget is null)
            {
                return;
            }

            var y = e.GetPosition(_header).Y;
            if (y >= 0 && y <= _header.ActualHeight)
            {
                return;
            }

            var header = _header;
            _header = null;

            DragDrop.DoDragDrop(header, new DataObject(_dataFormat, header.Column.SortMemberPath), DragDropEffects.Link);
        }

        private void OnDragOver(object sender, DragEventArgs e)
        {
            e.Effects = e.Data.GetDataPresent(_dataFormat) ? DragDropEffects.Link : DragDropEffects.None;
            e.Handled = true;
        }

        private async void OnDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(_dataFormat) is string propertyName)
            {
                e.Handled = true;
                await Drop(propertyName);
            }
        }

        internal async Task Drop(string propertyName)
        {
            if (Grouping is null)
            {
                return;
            }

            try
            {
                await Grouping.GroupBy(propertyName, CancellationToken.None);
            }
            catch (ArgumentException)
            {
                // a sort path doesn't have to be something the grouping offers, like a nested path
            }
        }
    }
}
