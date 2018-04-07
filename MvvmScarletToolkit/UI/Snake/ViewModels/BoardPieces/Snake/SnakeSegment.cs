using System.Diagnostics;

namespace MvvmScarletToolkit
{
    [DebuggerDisplay("Segment {Sequence},{CurrentPosition.X};{CurrentPosition.Y}")]
    public sealed class SnakeSegment : SnakeBase
    {
        public int Sequence { get; }

        public SnakeSegment(SnakeOptions options, IPositionable positionable, ILogger log, int sequence)
            : base(options, log)
        {
            CurrentPosition = positionable.CurrentPosition;
            Sequence = sequence;
        }
    }
}
