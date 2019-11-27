using MvvmScarletToolkit.Abstractions;
using MvvmScarletToolkit.Commands;
using MvvmScarletToolkit.Observables;
using System.Windows;
using System.Windows.Input;

namespace DemoApp
{
    public class SyncViewModel : ObservableObject
    {
        public ICommand WorkCommand { get; }

        public SyncViewModel(IScarletCommandManager commandManager)
        {
            WorkCommand = new RelayCommand<string>(commandManager, (arg) => Work(arg));
        }

        private void Work(string arg)
        {
            MessageBox.Show(arg);
        }
    }
}
