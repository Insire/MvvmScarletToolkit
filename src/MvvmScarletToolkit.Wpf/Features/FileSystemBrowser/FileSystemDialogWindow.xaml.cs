using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace MvvmScarletToolkit.Wpf.Features.FileSystemBrowser
{
    public partial class FileSystemDialogWindow : Window
    {
        private readonly FileSystemPickerViewModel _viewModel;

        public FileSystemDialogWindow(FileSystemPickerViewModel viewModel)
        {
            ArgumentNullException.ThrowIfNull(viewModel);

            _viewModel = viewModel;
            _viewModel.Confirmed += OnConfirmed;

            DataContext = viewModel;

            InitializeComponent();
        }

        /// <summary>
        /// Saving starts with the file name, like the save dialog of the operating system.
        /// </summary>
        private void FileName_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                textBox.Focus();

                // select the name without its extension, so typing replaces only the name
                var extension = Path.GetExtension(textBox.Text);
                textBox.Select(0, textBox.Text.Length - extension.Length);
            }
        }

        private void OnConfirmed(object? sender, EventArgs e)
        {
            DialogResult = true;
        }

        protected override void OnClosed(EventArgs e)
        {
            _viewModel.Confirmed -= OnConfirmed;

            base.OnClosed(e);
        }
    }
}
