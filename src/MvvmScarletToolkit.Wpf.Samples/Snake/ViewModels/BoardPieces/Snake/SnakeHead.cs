using Microsoft.Toolkit.Mvvm.Messaging;
using System.Diagnostics;

namespace MvvmScarletToolkit.Wpf.Samples
{
    [DebuggerDisplay("Head {CurrentPosition.X};{CurrentPosition.Y}")]
    public sealed class SnakeHead : SnakeBase
    {
        public SnakeHead(SnakeOption options, IMessenger messenger)
            : base(options, messenger)
        {
            CurrentPosition = options.GetStartingPosition();
        }
    }
}
