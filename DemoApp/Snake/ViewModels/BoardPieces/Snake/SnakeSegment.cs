using System.Diagnostics;

namespace MvvmScarletToolkit
{
    [DebuggerDisplay("Segment {Sequence},{CurrentPosition.X};{CurrentPosition.Y}")]
    public sealed class SnakeSegment : SnakeBase
    {
        public int Sequence { get; }

        public SnakeSegment(SnakeOption options, IPositionable positionable, IMessenger log, int sequence)
            : base(options, log)
        {
            CurrentPosition = positionable.CurrentPosition;
            Sequence = sequence;
        }
    }
}
