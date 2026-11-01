using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using MvvmScarletToolkit.Core.Samples.Features.Image;
using MvvmScarletToolkit.Observables;
using System.Diagnostics;

namespace MvvmScarletToolkit.Avalonia.Samples.Views
{
    public partial class DragAndDropView : UserControl
    {
        private const double MinimumDragDistance = 4;

        private static readonly DataFormat<ImageViewModel> ImageFormat = DataFormat.CreateInProcessFormat<ImageViewModel>("ImageViewModel");

        // the left mouse button press, null if there is none that can start a drag
        private PointerPressedEventArgs? _press;
        private Point _dragStart;

        public DragAndDropView()
        {
            InitializeComponent();

            SourceList.AddHandler(DragDrop.DragOverEvent, OnDragOver);
            TargetList.AddHandler(DragDrop.DragOverEvent, OnDragOver);
            SourceList.AddHandler(DragDrop.DropEvent, SourceList_Drop);
            TargetList.AddHandler(DragDrop.DropEvent, TargetList_Drop);
        }

        private void Element_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            // presses on a button inside the item are clicks, not the start of a drag
            var canDrag = sender is Control element
                && e.GetCurrentPoint(element).Properties.IsLeftButtonPressed
                && (e.Source as Control)?.FindAncestorOfType<Button>(includeSelf: true) is null;

            _press = canDrag ? e : null;
            _dragStart = e.GetPosition(this);
        }

        private void Element_PointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            _press = null;
        }

        private async void Element_PointerMoved(object? sender, PointerEventArgs e)
        {
            if (_press is not { } press || sender is not Control { DataContext: ImageViewModel image })
            {
                return;
            }

            var delta = e.GetPosition(this) - _dragStart;
            if (Math.Abs(delta.X) < MinimumDragDistance && Math.Abs(delta.Y) < MinimumDragDistance)
            {
                return;
            }

            _press = null;

            var data = new DataTransfer();
            data.Add(DataTransferItem.Create(ImageFormat, image));

            await DragDrop.DoDragDropAsync(press, data, DragDropEffects.Copy | DragDropEffects.Move);
        }

        private static void OnDragOver(object? sender, DragEventArgs e)
        {
            // Copy if CTRL is pressed; otherwise, move.
            e.DragEffects = e.DataTransfer.Contains(ImageFormat)
                ? e.KeyModifiers.HasFlag(KeyModifiers.Control) ? DragDropEffects.Copy : DragDropEffects.Move
                : DragDropEffects.None;
        }

        private void TargetList_Drop(object? sender, DragEventArgs e)
        {
            if (DataContext is ProcessingImagesViewModel vm)
            {
                OnDrop(e, vm.Source, vm.Target);
            }
        }

        private void SourceList_Drop(object? sender, DragEventArgs e)
        {
            if (DataContext is ProcessingImagesViewModel vm)
            {
                OnDrop(e, vm.Target, vm.Source);
            }
        }

        private static void OnDrop(DragEventArgs e, ViewModelListBase<ImageViewModel> from, ViewModelListBase<ImageViewModel> to)
        {
            if (e.DataTransfer.TryGetValue(ImageFormat) is not { } image)
            {
                return;
            }

            // Effects and Handled have to be set before the first await, the event is over afterwards
            var copy = e.KeyModifiers.HasFlag(KeyModifiers.Control);
            e.DragEffects = copy ? DragDropEffects.Copy : DragDropEffects.Move;
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
    }
}
