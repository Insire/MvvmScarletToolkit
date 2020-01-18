using MvvmScarletToolkit.Abstractions;
using System.Diagnostics;

namespace MvvmScarletToolkit.Samples
{
    [DebuggerDisplay("Head {CurrentPosition.X};{CurrentPosition.Y}")]
    public sealed class SnakeHead : SnakeBase
    {
        public SnakeHead(SnakeOption options, IScarletMessenger messenger)
            : base(options, messenger)
        {
            CurrentPosition = options.GetStartingPosition();
        }
    }
}
