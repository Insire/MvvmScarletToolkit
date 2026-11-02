using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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

        protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            base.OnMouseDoubleClick(e);

            // a double click on the scrollbar or a column header must not open the selected entry
            if ((e.OriginalSource as DependencyObject)?.FindParent<ListBoxItem>() is { } item)
            {
                ViewModel?.OpenCommand.Execute(item.DataContext);
            }
        }
    }
}
