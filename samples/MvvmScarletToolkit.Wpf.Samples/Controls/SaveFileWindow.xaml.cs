using MvvmScarletToolkit.Wpf.Samples.Features.DataGrid;
using System.IO;
using System.Windows;

namespace MvvmScarletToolkit.Wpf.Samples.Controls
{
    public partial class SaveFileWindow : Window
    {
        public SaveFileWindow()
        {
            InitializeComponent();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not SaveFileViewModel viewModel)
            {
                return;
            }

            if (File.Exists(viewModel.FilePath)
                && MessageBox.Show(this, $"{viewModel.FilePath} already exists. Replace it?", Title, MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
            {
                return;
            }

            DialogResult = true;
        }
    }
}
