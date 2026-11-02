using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;

namespace MvvmScarletToolkit
{
    /// <summary>
    /// A button for a column header, that opens a popup to configure a <see cref="ColumnFilterViewModel"/>.
    /// The filter is applied when the popup closes. <see cref="Control.Background"/> and <see cref="Control.Foreground"/> are the colors of the popup.
    /// </summary>
    [TemplatePart(Name = PopupPart, Type = typeof(Popup))]
    [TemplatePart(Name = TextPart, Type = typeof(UIElement))]
    [TemplatePart(Name = OptionsPart, Type = typeof(UIElement))]
    public sealed class DataGridColumnFilter : Control
    {
        private const string PopupPart = "PART_Popup";
        private const string TextPart = "PART_Text";
        private const string OptionsPart = "PART_Options";
        private const int ReopenDelay = 300;

        private Popup? _popup;
        private long _closedAt;

        public ColumnFilterViewModel? Filter
        {
            get { return (ColumnFilterViewModel?)GetValue(FilterProperty); }
            set { SetValue(FilterProperty, value); }
        }

        public static readonly DependencyProperty FilterProperty = DependencyProperty.Register(
            nameof(Filter),
            typeof(ColumnFilterViewModel),
            typeof(DataGridColumnFilter),
            new PropertyMetadata(null));

        public bool IsOpen
        {
            get { return (bool)GetValue(IsOpenProperty); }
            set { SetValue(IsOpenProperty, value); }
        }

        public static readonly DependencyProperty IsOpenProperty = DependencyProperty.Register(
            nameof(IsOpen),
            typeof(bool),
            typeof(DataGridColumnFilter),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsOpenChanged));
        private static void OnIsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DataGridColumnFilter filter && e.NewValue is false)
            {
                filter._closedAt = Environment.TickCount64;
            }
        }

        public override void OnApplyTemplate()
        {
            if (_popup is not null)
            {
                _popup.Opened -= OnPopupOpened;
                _popup.Closed -= OnPopupClosed;
            }

            base.OnApplyTemplate();

            _popup = GetTemplateChild(PopupPart) as Popup;

            if (_popup is not null)
            {
                _popup.Opened += OnPopupOpened;
                _popup.Closed += OnPopupClosed;
            }
        }

        private void OnPopupOpened(object? sender, EventArgs e)
        {
            // the content can't take the focus until it is displayed
            Dispatcher.BeginInvoke(DispatcherPriority.Input, FocusInput);
        }

        private void OnPopupClosed(object? sender, EventArgs e)
        {
            Filter?.Apply();

            // filtering can take longer than the delay, while the click is still waiting in the input queue
            _closedAt = Environment.TickCount64;
        }

        private void FocusInput()
        {
            var input = GetTemplateChild(Filter?.Options is null ? TextPart : OptionsPart) as UIElement;

            input?.Focus();
        }

        // routed events of the popup continue through this control. Without stopping them, the column header would sort and the grid would navigate.
        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            // the popup closes itself on the same click, which would then open it again through the button
            if (!new Rect(RenderSize).Contains(e.GetPosition(this)))
            {
                return;
            }

            if (IsOpen)
            {
                IsOpen = false;
                e.Handled = true;
            }
            else if (Environment.TickCount64 - _closedAt < ReopenDelay)
            {
                e.Handled = true;
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (!IsOpen)
            {
                return;
            }

            if (e.Key is Key.Enter or Key.Escape)
            {
                IsOpen = false;
                e.Handled = true;
            }
            else if (e.Key is Key.Up or Key.Down or Key.Left or Key.Right or Key.PageUp or Key.PageDown or Key.Home or Key.End or Key.Delete or Key.F2)
            {
                e.Handled = true;
            }
        }
    }
}
