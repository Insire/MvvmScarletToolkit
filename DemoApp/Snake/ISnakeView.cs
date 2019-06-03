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
        IConcurrentCommand ShowGameCommand { get; set; }
        IConcurrentCommand ShowOptionsCommand { get; set; }
        IConcurrentCommand ShowStartCommand { get; set; }
        SnakeViewModel SnakeViewModel { get; set; }
        View View { get; set; }
    }
}
