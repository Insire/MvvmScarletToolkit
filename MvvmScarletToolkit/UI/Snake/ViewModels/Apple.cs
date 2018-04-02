using System;
using System.Diagnostics;

using MvvmScarletToolkit.SnakeGame;

namespace MvvmScarletToolkit
{
    [DebuggerDisplay("Apple {CurrentPosition.X};{CurrentPosition.Y}")]
    public sealed class Apple : ObservableObject, IPositionable
    {
        private Position _currentPosition;
        public Position CurrentPosition
        {
            get { return _currentPosition; }
            private set { SetValue(ref _currentPosition, value); }
        }

        private Size _size;
        public Size Size
        {
            get { return _size; }
            private set { SetValue(ref _size, value); }
        }

        public Apple(Position position, Size size)
        {
            CurrentPosition = position ?? throw new ArgumentNullException(nameof(position));
            Size = size ?? throw new ArgumentNullException(nameof(size));
        }

        public Apple(int x, int y, SnakeOptions options)
            : this(new Position { X = x, Y = y, }, new Size(options.FieldSize, options.FieldSize))
        {
        }
    }
}
