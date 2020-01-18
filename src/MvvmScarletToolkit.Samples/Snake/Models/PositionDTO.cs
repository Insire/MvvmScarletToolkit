using System.Diagnostics;

namespace MvvmScarletToolkit.Samples
{
    [DebuggerDisplay("PositionModel {CurrentPosition.X};{CurrentPosition.Y}")]
    internal class PositionDTO : IPositionable
    {
        public Position CurrentPosition { get; set; }
        public Size Size { get; }

        public PositionDTO(IPositionable positionable)
        {
            CurrentPosition = positionable.CurrentPosition;
            Size = positionable.Size;
        }
    }
}
