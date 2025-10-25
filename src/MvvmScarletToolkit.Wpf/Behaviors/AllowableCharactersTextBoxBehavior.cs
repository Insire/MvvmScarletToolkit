using Microsoft.Xaml.Behaviors;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MvvmScarletToolkit
{
    /// <summary>
    /// Limits <see cref="TextBox.Text"/> to have a maximum length and/or it having to satisfy a given <see cref="System.Text.RegularExpressions.Regex"/>
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
    //    <mvvm:AllowableCharactersTextBoxBehavior MaxLength = "4", RegularExpression="[^a-zA-Z]" />
    // </ i:Interaction.Behaviors>
    // https://stackoverflow.com/questions/1268552/how-do-i-get-a-textbox-to-only-accept-numeric-input-in-wpf/8015485#8015485
    public sealed class AllowableCharactersTextBoxBehavior : Behavior<TextBox>
    {
        /// <summary>Identifies the <see cref="RegularExpression"/> dependency property.</summary>
        public static readonly DependencyProperty RegularExpressionProperty = DependencyProperty.Register(
            nameof(RegularExpression),
            typeof(string),
            typeof(AllowableCharactersTextBoxBehavior),
            new FrameworkPropertyMetadata(".*"));

        public string RegularExpression
        {
            get => (string)GetValue(RegularExpressionProperty);
            set => SetValue(RegularExpressionProperty, value);
        }

        /// <summary>Identifies the <see cref="MaxLength"/> dependency property.</summary>
        public static readonly DependencyProperty MaxLengthProperty = DependencyProperty.Register(
            nameof(MaxLength),
            typeof(int),
            typeof(AllowableCharactersTextBoxBehavior),
            new FrameworkPropertyMetadata(int.MaxValue));

        public int MaxLength
        {
            get => (int)GetValue(MaxLengthProperty);
            set => SetValue(MaxLengthProperty, value);
        }

        /// <summary>Identifies the <see cref="AllowWhitespace"/> dependency property.</summary>
        public static readonly DependencyProperty AllowWhitespaceProperty =
            DependencyProperty.Register(nameof(AllowWhitespace),
                typeof(bool),
                typeof(AllowableCharactersTextBoxBehavior),
                new PropertyMetadata(true));

        public bool AllowWhitespace
        {
            get => (bool)GetValue(AllowWhitespaceProperty);
            set => SetValue(AllowWhitespaceProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.PreviewKeyDown += OnPreviewKeyDown;
            AssociatedObject.PreviewTextInput += OnPreviewTextInput;

            DataObject.AddPastingHandler(AssociatedObject, OnPaste);
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            AssociatedObject.PreviewKeyDown -= OnPreviewKeyDown;
            AssociatedObject.PreviewTextInput -= OnPreviewTextInput;

            DataObject.RemovePastingHandler(AssociatedObject, OnPaste);
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (AllowWhitespace)
            {
                return;
            }

            if (e.Key == Key.Space)
            {
                e.Handled = true;
            }
        }

        private void OnPaste(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(DataFormats.Text))
            {
                var newText = Convert.ToString(e.DataObject.GetData(DataFormats.Text));

                var selectedTextLength = AssociatedObject.SelectedText.Length;
                var caretIndex = AssociatedObject.CaretIndex;
                var text = AssociatedObject.Text;
                var isInsertKeyToggled = Keyboard.IsKeyToggled(Key.Insert);

                if (!AllowableCharactersTextBoxBehaviorService.IsValid(newText, true, selectedTextLength, MaxLength, text, caretIndex, RegularExpression, isInsertKeyToggled))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        private void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var selectedTextLength = AssociatedObject.SelectedText.Length;
            var caretIndex = AssociatedObject.CaretIndex;
            var text = AssociatedObject.Text;
            var isInsertKeyToggled = Keyboard.IsKeyToggled(Key.Insert);

            e.Handled = !AllowableCharactersTextBoxBehaviorService.IsValid(e.Text, false, selectedTextLength, MaxLength, text, caretIndex, RegularExpression, isInsertKeyToggled);
        }
    }
}
