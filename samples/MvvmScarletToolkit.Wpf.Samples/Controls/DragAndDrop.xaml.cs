using MvvmScarletToolkit.Observables;
using MvvmScarletToolkit.Wpf.Samples.Features;
using MvvmScarletToolkit.Wpf.Samples.Features.Image;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace MvvmScarletToolkit.Wpf.Samples.Controls
{
    public partial class DragAndDrop : UserControl
    {
        // where the left mouse button was pressed, null if the press can't start a drag
        private Point? _dragStart;

        public DragAndDrop()
        {
            InitializeComponent();
        }

        private void Element_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // presses on a button inside the item are clicks, not the start of a drag
            _dragStart = IsWithinButton(e.OriginalSource as DependencyObject, sender as DependencyObject)
                ? null
                : e.GetPosition(this);
        }

        private void Element_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed || _dragStart is not Point start || sender is not FrameworkElement element)
            {
                return;
            }

            var delta = e.GetPosition(this) - start;
            if (Math.Abs(delta.X) < SystemParameters.MinimumHorizontalDragDistance
                && Math.Abs(delta.Y) < SystemParameters.MinimumVerticalDragDistance)
            {
                return;
            }

            _dragStart = null;

            // Package the data.
            var data = new DataObject();
            data.SetData(element.DataContext);

            // Inititate the drag-and-drop operation.
            // the element is the drag source, so that its GiveFeedback handler gets called
            DragDrop.DoDragDrop(element, data, DragDropEffects.Copy | DragDropEffects.Move);
        }

        private void Element_GiveFeedback(object sender, GiveFeedbackEventArgs e)
        {
            // These Effects values are set in the drop target's
            // DragOver event handler.
            if ((e.Effects & DragDropEffects.Copy) != 0)
            {
                Mouse.SetCursor(Cursors.Cross);
            }
            else if ((e.Effects & DragDropEffects.Move) != 0)
            {
                Mouse.SetCursor(Cursors.Pen);
            }
            else
            {
                Mouse.SetCursor(Cursors.No);
            }
            e.Handled = true;
        }

        private void TargetList_Drop(object sender, DragEventArgs e)
        {
            if (DataContext is ProcessingImagesViewModel vm)
            {
                OnDrop(e, vm.Source, vm.Target);
            }
        }

        private void SourceList_Drop(object sender, DragEventArgs e)
        {
            if (DataContext is ProcessingImagesViewModel vm)
            {
                OnDrop(e, vm.Target, vm.Source);
            }
        }

        private static void OnDrop(DragEventArgs e, ViewModelListBase<ImageViewModel> from, ViewModelListBase<ImageViewModel> to)
        {
            if (e.Data.GetData(typeof(ImageViewModel)) is not ImageViewModel image)
            {
                return;
            }

            // Set Effects to notify the drag source what effect
            // the drag-and-drop operation had.
            // (Copy if CTRL is pressed; otherwise, move.)
            // Effects and Handled have to be set before the first await, the event is over afterwards
            var copy = (e.KeyStates & DragDropKeyStates.ControlKey) != 0;
            e.Effects = copy ? DragDropEffects.Copy : DragDropEffects.Move;
            e.Handled = true;

            _ = Transfer(image, from, to, copy);
        }

        private static async Task Transfer(ImageViewModel image, ViewModelListBase<ImageViewModel> from, ViewModelListBase<ImageViewModel> to, bool copy)
        {
            try
            {
                // no ConfigureAwait(false), Items may only be read on the UI thread, which modifies it
                if (!copy)
                {
                    await from.Remove(image);
                }

                if (!to.Items.Contains(image))
                {
                    await to.Add(image);
                }
            }
            catch (Exception ex)
            {
                // there is nobody to observe this task, don't let a failure go unnoticed or crash the application
                Trace.TraceError("Dropping {0} failed: {1}", image.DisplayName, ex);
            }
        }

        private static bool IsWithinButton(DependencyObject? element, DependencyObject? root)
        {
            while (element is not null && !ReferenceEquals(element, root))
            {
                if (element is ButtonBase)
                {
                    return true;
                }

                element = element is Visual or System.Windows.Media.Media3D.Visual3D
                    ? VisualTreeHelper.GetParent(element)
                    : LogicalTreeHelper.GetParent(element);
            }

            return false;
        }
    }
}
