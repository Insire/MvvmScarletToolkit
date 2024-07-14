using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MvvmScarletToolkit.Wpf.Samples
{
    public partial class DragAndDrop : UserControl
    {
        public DragAndDrop()
        {
            InitializeComponent();
        }

        private void Element_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                // Package the data.
                var data = new DataObject();
                data.SetData((sender as FrameworkElement)?.DataContext);

                // Inititate the drag-and-drop operation.
                DragDrop.DoDragDrop(this, data, DragDropEffects.Copy | DragDropEffects.Move);
            }
        }

        private void Element_GiveFeedback(object sender, GiveFeedbackEventArgs e)
        {
            base.OnGiveFeedback(e);
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

        private async void TargetList_Drop(object sender, DragEventArgs e)
        {
            base.OnDrop(e);

            // If the DataObject contains string data, extract it.
            if (e.Data.GetDataPresent(typeof(ImageViewModel)))
            {
                var image = (ImageViewModel)e.Data.GetData(typeof(ImageViewModel));

                if (DataContext is ProcessingImagesViewModel vm)
                {
                    // Set Effects to notify the drag source what effect
                    // the drag-and-drop operation had.
                    // (Copy if CTRL is pressed; otherwise, move.)
                    if ((e.KeyStates & DragDropKeyStates.ControlKey) != 0)
                    {
                        e.Effects = DragDropEffects.Copy;
                        await vm.Target.Add(image).ConfigureAwait(false);
                    }
                    else
                    {
                        e.Effects = DragDropEffects.Move;
                        await vm.Source.Remove(image).ConfigureAwait(false);
                        await vm.Target.Add(image).ConfigureAwait(false);
                    }
                }
            }

            e.Handled = true;
        }

        private async void SourceList_Drop(object sender, DragEventArgs e)
        {
            base.OnDrop(e);

            // If the DataObject contains string data, extract it.
            if (e.Data.GetDataPresent(typeof(ImageViewModel)))
            {
                var image = (ImageViewModel)e.Data.GetData(typeof(ImageViewModel));

                if (DataContext is ProcessingImagesViewModel vm)
                {
                    // Set Effects to notify the drag source what effect
                    // the drag-and-drop operation had.
                    // (Copy if CTRL is pressed; otherwise, move.)
                    if ((e.KeyStates & DragDropKeyStates.ControlKey) != 0)
                    {
                        e.Effects = DragDropEffects.Copy;
                        await vm.Source.Add(image).ConfigureAwait(false);
                    }
                    else
                    {
                        e.Effects = DragDropEffects.Move;
                        await vm.Target.Remove(image).ConfigureAwait(false);
                        await vm.Source.Add(image).ConfigureAwait(false);
                    }
                }
            }

            e.Handled = true;
        }
    }
}
