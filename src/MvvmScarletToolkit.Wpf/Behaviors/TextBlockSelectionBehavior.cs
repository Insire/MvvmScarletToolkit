using Microsoft.Xaml.Behaviors;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace MvvmScarletToolkit.Wpf
{
    /// <summary>
    /// Lets users select the text of a TextBlock with the mouse, a word by double clicking it, and copy it, through Ctrl+C or a default context menu.
    /// The selection is drawn in an adorner, so the inlines and with it their foregrounds and bindings stay untouched.
    /// Its appearance and the selected text are available through the attached properties of <see cref="TextBlockSelection"/>,
    /// whose <see cref="TextBlockSelection.IsEnabledProperty"/> adds this behavior from a style.
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
    public sealed class TextBlockSelectionBehavior : Behavior<TextBlock>
    {
        private readonly CommandBinding _copyBinding;
        private readonly CommandBinding _selectAllBinding;

        private TextRange? _selection;
        private TextPointer? _anchor;
        private AdornerLayer? _adornerLayer;
        private SelectionAdorner? _adorner;
        private ContextMenu? _contextMenu;
        private bool _isTrackingLayout;
        private bool _hasSetCursor;

        public TextBlockSelectionBehavior()
        {
            _copyBinding = new CommandBinding(ApplicationCommands.Copy, OnCopyExecuted, OnCopyCanExecute);
            _selectAllBinding = new CommandBinding(ApplicationCommands.SelectAll, OnSelectAllExecuted);
        }

        /// <summary>
        /// Selects the text between two positions of the associated TextBlock, in any order
        /// </summary>
        public void Select(TextPointer start, TextPointer end)
        {
            ArgumentNullException.ThrowIfNull(start);
            ArgumentNullException.ThrowIfNull(end);

            if (start.CompareTo(end) > 0)
            {
                (start, end) = (end, start);
            }

            if (_selection is null)
            {
                _selection = new TextRange(start, end);
            }
            else
            {
                _selection.Select(start, end);
            }

            UpdateSelection();
        }

        /// <summary>
        /// Selects the word at a position of the associated TextBlock. Without a word next to the position, the selection is removed.
        /// </summary>
        public void SelectWord(TextPointer position)
        {
            ArgumentNullException.ThrowIfNull(position);

            Select(FindWordEdge(position, LogicalDirection.Backward), FindWordEdge(position, LogicalDirection.Forward));
        }

        /// <summary>
        /// Selects the entire text
        /// </summary>
        public void SelectAll()
        {
            if (AssociatedObject is not null)
            {
                Select(AssociatedObject.ContentStart, AssociatedObject.ContentEnd);
            }
        }

        /// <summary>
        /// Removes the selection, without changing the text
        /// </summary>
        public void ClearSelection()
        {
            if (_selection is not null)
            {
                _selection.Select(_selection.Start, _selection.Start);
                UpdateSelection();
            }
        }

        /// <summary>
        /// Puts the selected text on the clipboard
        /// </summary>
        public void Copy()
        {
            var text = _selection?.Text;
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            try
            {
                Clipboard.SetDataObject(text, true);
            }
            catch (ExternalException)
            {
                // another process is holding on to the clipboard, which is no reason to take the application down
            }
        }

        /// <summary>
        /// Redraws the selection, after its appearance changed
        /// </summary>
        internal void InvalidateSelection()
        {
            _adorner?.InvalidateVisual();
        }

        /// <summary>
        /// The areas of the selected text, relative to the TextBlock. Text spanning multiple lines has one area per line.
        /// </summary>
        internal IReadOnlyList<Rect> GetSelectionAreas()
        {
            var result = new List<Rect>();
            if (_selection is null || _selection.IsEmpty)
            {
                return result;
            }

            var pointer = _selection.Start;
            while (pointer.CompareTo(_selection.End) < 0)
            {
                var next = pointer.GetPositionAtOffset(1, LogicalDirection.Forward);
                if (next is null)
                {
                    break;
                }

                if (pointer.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
                {
                    // at a line wrap the same position is the end of a line going backward and the start of the next line going forward
                    var area = Rect.Union(pointer.GetCharacterRect(LogicalDirection.Forward), next.GetCharacterRect(LogicalDirection.Backward));
                    if (result.Count > 0 && IsSameLine(result[^1], area))
                    {
                        result[^1] = Rect.Union(result[^1], area);
                    }
                    else
                    {
                        result.Add(area);
                    }
                }

                pointer = next;
            }

            return result;
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            // current values, so that whatever a style or the application has set comes back when detaching
            AssociatedObject.SetCurrentValue(UIElement.FocusableProperty, true);
            AssociatedObject.SetCurrentValue(FrameworkElement.FocusVisualStyleProperty, null);
            AssociatedObject.SetCurrentValue(KeyboardNavigation.IsTabStopProperty, false);

            if (AssociatedObject.Cursor is null)
            {
                AssociatedObject.SetCurrentValue(FrameworkElement.CursorProperty, Cursors.IBeam);
                _hasSetCursor = true;
            }

            AssociatedObject.CommandBindings.Add(_copyBinding);
            AssociatedObject.CommandBindings.Add(_selectAllBinding);

            AssociatedObject.MouseLeftButtonDown += OnMouseLeftButtonDown;
            AssociatedObject.MouseMove += OnMouseMove;
            AssociatedObject.MouseLeftButtonUp += OnMouseLeftButtonUp;
            AssociatedObject.LostMouseCapture += OnLostMouseCapture;
            AssociatedObject.MouseRightButtonDown += OnMouseRightButtonDown;
            AssociatedObject.ContextMenuOpening += OnContextMenuOpening;
            AssociatedObject.LostFocus += OnLostFocus;
            AssociatedObject.Unloaded += OnUnloaded;
            AssociatedObject.IsVisibleChanged += OnIsVisibleChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            ClearSelection();
            _selection = null;

            AssociatedObject.IsVisibleChanged -= OnIsVisibleChanged;
            AssociatedObject.Unloaded -= OnUnloaded;
            AssociatedObject.LostFocus -= OnLostFocus;
            AssociatedObject.ContextMenuOpening -= OnContextMenuOpening;
            AssociatedObject.MouseRightButtonDown -= OnMouseRightButtonDown;
            AssociatedObject.LostMouseCapture -= OnLostMouseCapture;
            AssociatedObject.MouseLeftButtonUp -= OnMouseLeftButtonUp;
            AssociatedObject.MouseMove -= OnMouseMove;
            AssociatedObject.MouseLeftButtonDown -= OnMouseLeftButtonDown;

            AssociatedObject.CommandBindings.Remove(_selectAllBinding);
            AssociatedObject.CommandBindings.Remove(_copyBinding);

            if (_contextMenu is not null && ReferenceEquals(AssociatedObject.ContextMenu, _contextMenu))
            {
                AssociatedObject.ClearValue(FrameworkElement.ContextMenuProperty);
            }

            _contextMenu = null;

            if (_hasSetCursor)
            {
                AssociatedObject.InvalidateProperty(FrameworkElement.CursorProperty);
                _hasSetCursor = false;
            }

            AssociatedObject.InvalidateProperty(KeyboardNavigation.IsTabStopProperty);
            AssociatedObject.InvalidateProperty(FrameworkElement.FocusVisualStyleProperty);
            AssociatedObject.InvalidateProperty(UIElement.FocusableProperty);
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // the commands are routed from the focused element
            AssociatedObject.Focus();

            var position = AssociatedObject.GetPositionFromPoint(e.GetPosition(AssociatedObject), true);
            if (position is null)
            {
                return;
            }

            if (e.ClickCount == 2)
            {
                SelectWord(position);
            }
            else
            {
                _anchor = position;
                Select(_anchor, _anchor);
                AssociatedObject.CaptureMouse();
            }

            // parents like a ListBoxItem would take the focus otherwise, which ends the selection
            e.Handled = true;
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (_anchor is null)
            {
                return;
            }

            if (e.LeftButton != MouseButtonState.Pressed)
            {
                AssociatedObject.ReleaseMouseCapture();
                return;
            }

            var position = AssociatedObject.GetPositionFromPoint(e.GetPosition(AssociatedObject), true);
            if (position is not null)
            {
                Select(_anchor, position);
            }
        }

        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            AssociatedObject.ReleaseMouseCapture();
        }

        private void OnLostMouseCapture(object sender, MouseEventArgs e)
        {
            _anchor = null;
        }

        private void OnMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            AssociatedObject.Focus();
        }

        private void OnContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            CommandManager.InvalidateRequerySuggested();

            if (AssociatedObject.ContextMenu is not null)
            {
                return;
            }

            _contextMenu = CreateContextMenu();
            AssociatedObject.ContextMenu = _contextMenu;

            // wpf looks for a context menu before instance handlers run, so this first one wouldn't show up on its own
            _contextMenu.PlacementTarget = AssociatedObject;
            _contextMenu.IsOpen = true;
            e.Handled = true;
        }

        private void OnLostFocus(object sender, RoutedEventArgs e)
        {
            // opening the context menu only moves the keyboard focus, the logical focus stays here
            ClearSelection();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            ClearSelection();
        }

        private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!AssociatedObject.IsVisible)
            {
                ClearSelection();
            }
        }

        // TextRange.Changed ignores changes from outside of the range and a TextBlock raises nothing on its own,
        // so a new layout is the only sign of a text change, like from a binding
        private void OnLayoutUpdated(object? sender, EventArgs e)
        {
            if (_selection?.Text != TextBlockSelection.GetSelectedText(AssociatedObject))
            {
                ClearSelection();
            }
        }

        private ContextMenu CreateContextMenu()
        {
            var menu = new ContextMenu();
            menu.Items.Add(new MenuItem { Command = ApplicationCommands.Copy, CommandTarget = AssociatedObject });
            menu.Items.Add(new MenuItem { Command = ApplicationCommands.SelectAll, CommandTarget = AssociatedObject });

            return menu;
        }

        private void UpdateSelection()
        {
            TextBlockSelection.SetSelectedText(AssociatedObject, _selection?.Text ?? string.Empty);

            var hasSelection = _selection is not null && !_selection.IsEmpty;
            if (hasSelection != _isTrackingLayout)
            {
                _isTrackingLayout = hasSelection;

                if (hasSelection)
                {
                    AssociatedObject.LayoutUpdated += OnLayoutUpdated;
                }
                else
                {
                    AssociatedObject.LayoutUpdated -= OnLayoutUpdated;
                }
            }

            if (!hasSelection)
            {
                RemoveAdorner();
            }
            else if (_adorner is not null)
            {
                _adorner.InvalidateVisual();
            }
            else
            {
                AddAdorner();
            }
        }

        private void AddAdorner()
        {
            _adornerLayer = AdornerLayer.GetAdornerLayer(AssociatedObject);
            if (_adornerLayer is null)
            {
                return;
            }

            _adorner = new SelectionAdorner(AssociatedObject, this);
            _adornerLayer.Add(_adorner);
        }

        private void RemoveAdorner()
        {
            if (_adorner is not null)
            {
                _adornerLayer?.Remove(_adorner);
                _adorner = null;
                _adornerLayer = null;
            }
        }

        private void OnCopyCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = TextBlockSelection.GetSelectedText(AssociatedObject).Length > 0;
            e.Handled = true;
        }

        private void OnCopyExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            Copy();
            e.Handled = true;
        }

        private void OnSelectAllExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            SelectAll();
            e.Handled = true;
        }

        // a word can continue in the next inline, e.g. when only a part of it is bold
        private static TextPointer FindWordEdge(TextPointer position, LogicalDirection direction)
        {
            var isForward = direction == LogicalDirection.Forward;

            while (true)
            {
                switch (position.GetPointerContext(direction))
                {
                    case TextPointerContext.Text:
                        var text = position.GetTextInRun(direction);
                        var length = 0;
                        while (length < text.Length && IsWordCharacter(text[isForward ? length : text.Length - 1 - length]))
                        {
                            length++;
                        }

                        position = position.GetPositionAtOffset(isForward ? length : -length);
                        if (length < text.Length)
                        {
                            return position;
                        }

                        break;

                    case TextPointerContext.ElementStart:
                    case TextPointerContext.ElementEnd:
                        if (position.GetAdjacentElement(direction) is LineBreak or InlineUIContainer)
                        {
                            return position;
                        }

                        position = position.GetNextContextPosition(direction);
                        break;

                    default:
                        return position;
                }
            }
        }

        private static bool IsWordCharacter(char character)
        {
            return char.IsLetterOrDigit(character)
                || char.IsSurrogate(character)
                || character == '_'
                || char.GetUnicodeCategory(character) == UnicodeCategory.NonSpacingMark;
        }

        private static bool IsSameLine(Rect first, Rect second)
        {
            return Math.Abs(first.Top - second.Top) < 0.01 && Math.Abs(first.Height - second.Height) < 0.01;
        }

        private sealed class SelectionAdorner : Adorner
        {
            private readonly TextBlock _textBlock;
            private readonly TextBlockSelectionBehavior _behavior;

            public SelectionAdorner(TextBlock textBlock, TextBlockSelectionBehavior behavior)
                : base(textBlock)
            {
                _textBlock = textBlock;
                _behavior = behavior;

                // mouse input has to reach the textblock, to keep changing the selection
                IsHitTestVisible = false;
            }

            protected override void OnRender(DrawingContext drawingContext)
            {
                base.OnRender(drawingContext);

                var brush = TextBlockSelection.GetSelectionBrush(_textBlock);

                drawingContext.PushOpacity(TextBlockSelection.GetSelectionOpacity(_textBlock));

                foreach (var area in _behavior.GetSelectionAreas())
                {
                    drawingContext.DrawRectangle(brush, null, area);
                }

                drawingContext.Pop();
            }
        }
    }
}
