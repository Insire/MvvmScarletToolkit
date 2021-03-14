using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System.Windows;
using System.Windows.Input;

namespace MvvmScarletToolkit.Wpf.Samples
{
    public class SyncViewModel : ObservableObject
    {
        public ICommand WorkCommand { get; }

        public SyncViewModel()
        {
            WorkCommand = new RelayCommand<string>((arg) => Work(arg));
        }

        private static void Work(string arg)
        {
            MessageBox.Show(arg);
        }
    }
}
