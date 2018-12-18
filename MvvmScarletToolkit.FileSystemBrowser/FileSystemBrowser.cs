using System.Windows;
using System.Windows.Controls;

namespace MvvmScarletToolkit.FileSystemBrowser
{
    public class FileSystemBrowser : Control
    {
        public FileSystemViewModel FileSystemViewModel
        {
            get { return (FileSystemViewModel)GetValue(FileSystemViewModelProperty); }
            set { SetValue(FileSystemViewModelProperty, value); }
        }

        public static readonly DependencyProperty FileSystemViewModelProperty = DependencyProperty.Register(
            nameof(FileSystemViewModel),
            typeof(FileSystemViewModel),
            typeof(FileSystemBrowser),
            new PropertyMetadata(default(FileSystemViewModel)));
    }
}
