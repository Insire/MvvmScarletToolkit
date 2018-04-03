using System.Diagnostics;

namespace MvvmScarletToolkit
{
    [DebuggerDisplay("Segment {CurrentPosition.X};{CurrentPosition.Y}")]
    public sealed class SnakeSegment : SnakeBase
    {
        public SnakeSegment(SnakeOptions options, SnakeHead head, ILogger log)
            : base(options, log)
        {
            CurrentPosition = head.CurrentPosition;
        }
    }
}
