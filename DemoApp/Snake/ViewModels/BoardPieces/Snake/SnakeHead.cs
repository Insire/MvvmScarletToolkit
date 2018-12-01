using MvvmScarletToolkit;
using System.Diagnostics;

namespace DemoApp
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
