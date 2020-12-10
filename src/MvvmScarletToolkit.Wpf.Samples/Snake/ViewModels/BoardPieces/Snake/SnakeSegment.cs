using Microsoft.Toolkit.Mvvm.Messaging;
using System.Diagnostics;

namespace MvvmScarletToolkit.Wpf.Samples
{
    [DebuggerDisplay("Segment {Sequence},{CurrentPosition.X};{CurrentPosition.Y}")]
    public sealed class SnakeSegment : SnakeBase
    {
        public int Sequence { get; }

        public SnakeSegment(SnakeOption options, IPositionable positionable, IMessenger messenger, int sequence)
            : base(options, messenger)
        {
            CurrentPosition = positionable.CurrentPosition;
            Sequence = sequence;
        }
    }
}
