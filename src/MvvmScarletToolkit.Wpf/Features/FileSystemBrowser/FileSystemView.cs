using System.Windows;
using System.Windows.Controls;

namespace MvvmScarletToolkit.Wpf.Features.FileSystemBrowser
{
    public class FileSystemView : Control
    {
        public FileSystemViewModel ViewModel
        {
            get { return (FileSystemViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
            nameof(ViewModel),
            typeof(FileSystemViewModel),
            typeof(FileSystemView),
            new PropertyMetadata(default(FileSystemViewModel)));
    }
}
