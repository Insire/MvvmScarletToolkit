using Avalonia.Controls;
using Avalonia.Interactivity;

namespace MvvmScarletToolkit.Avalonia.Samples.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void Button_Click(object? sender, RoutedEventArgs e)
    {
        GC.Collect();
    }
}
