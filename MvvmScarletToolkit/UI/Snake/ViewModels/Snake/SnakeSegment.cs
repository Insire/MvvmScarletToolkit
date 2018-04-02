using System.Diagnostics;

namespace MvvmScarletToolkit
{
    [DebuggerDisplay("Segment {CurrentPosition.X};{CurrentPosition.Y}")]
    public sealed class SnakeSegment : SnakeBase
    {
        public SnakeSegment(SnakeOptions options, SnakeHead head)
            : base(options)
        {
            CurrentPosition = head.CurrentPosition;
        }
    }
}
