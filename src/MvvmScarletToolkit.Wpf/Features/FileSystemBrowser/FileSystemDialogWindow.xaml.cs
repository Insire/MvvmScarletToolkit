using System.Windows;

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
