using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DemoApp
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
                DataObject data = new DataObject();
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
            if (e.Effects.HasFlag(DragDropEffects.Copy))
            {
                Mouse.SetCursor(Cursors.Cross);
            }
            else if (e.Effects.HasFlag(DragDropEffects.Move))
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
            base.OnDrop(e);

            // If the DataObject contains string data, extract it.
            if (e.Data.GetDataPresent(typeof(Image)))
            {
                var image = (Image)e.Data.GetData(typeof(Image));

                var vm = DataContext as ProcessingImagesViewModel;

                if (vm != null)
                {
                    // Set Effects to notify the drag source what effect
                    // the drag-and-drop operation had.
                    // (Copy if CTRL is pressed; otherwise, move.)
                    if (e.KeyStates.HasFlag(DragDropKeyStates.ControlKey))
                    {
                        e.Effects = DragDropEffects.Copy;
                        vm.Target.Add(image);
                    }
                    else
                    {
                        e.Effects = DragDropEffects.Move;
                        vm.Source.Remove(image);
                        vm.Target.Add(image);
                    }
                }

            }

            e.Handled = true;
        }

        private void SourceList_Drop(object sender, DragEventArgs e)
        {
            base.OnDrop(e);

            // If the DataObject contains string data, extract it.
            if (e.Data.GetDataPresent(typeof(Image)))
            {
                var image = (Image)e.Data.GetData(typeof(Image));

                var vm = DataContext as ProcessingImagesViewModel;

                if (vm != null)
                {
                    // Set Effects to notify the drag source what effect
                    // the drag-and-drop operation had.
                    // (Copy if CTRL is pressed; otherwise, move.)
                    if (e.KeyStates.HasFlag(DragDropKeyStates.ControlKey))
                    {
                        e.Effects = DragDropEffects.Copy;
                        vm.Source.Add(image);
                    }
                    else
                    {
                        e.Effects = DragDropEffects.Move;
                        vm.Target.Remove(image);
                        vm.Source.Add(image);
                    }
                }

            }

            e.Handled = true;
        }
    }
}
