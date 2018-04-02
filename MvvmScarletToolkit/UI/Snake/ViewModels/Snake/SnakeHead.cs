using System.Diagnostics;

namespace MvvmScarletToolkit
{
    [DebuggerDisplay("Head {CurrentPosition.X};{CurrentPosition.Y}")]
    public sealed class SnakeHead : SnakeBase
    {
        public SnakeHead(SnakeOptions options)
            : base(options)
        {
            CurrentPosition = options.GetStartingPosition();
        }
    }
}
