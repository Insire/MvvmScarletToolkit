using MvvmScarletToolkit.Abstractions;
using System.Windows.Input;

namespace DemoApp
{
    public interface ISnakeView
    {
        ICommand ExitCommand { get; set; }
        int FramesPerSecond { get; set; }
        bool IsFpsEnabled { get; set; }
        ISnakeManager Manager { get; set; }
        IAsyncCommand ShowGameCommand { get; set; }
        IAsyncCommand ShowOptionsCommand { get; set; }
        IAsyncCommand ShowStartCommand { get; set; }
        SnakeViewModel SnakeViewModel { get; set; }
        View View { get; set; }

        void InitializeComponent();
    }
}
