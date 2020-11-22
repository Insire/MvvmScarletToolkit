using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Diagnostics;

namespace MvvmScarletToolkit.Wpf.Samples
{
    [DebuggerDisplay("Apple {CurrentPosition.X};{CurrentPosition.Y}")]
    public sealed class Apple : ObservableObject, IPositionable
    {
        private Position _currentPosition;
        public Position CurrentPosition
        {
            get { return _currentPosition; }
            private set { SetProperty(ref _currentPosition, value); }
        }

        private Size _size;
        public Size Size
        {
            get { return _size; }
            private set { SetProperty(ref _size, value); }
        }

        public Apple(Position position, Size size)
        {
            CurrentPosition = position ?? throw new ArgumentNullException(nameof(position));
            Size = size ?? throw new ArgumentNullException(nameof(size));
        }

        public Apple(int x, int y, SnakeOption options)
            : this(new Position { X = x, Y = y, }, new Size(options.BoardPieceSize, options.BoardPieceSize))
        {
        }

        public Apple(IPositionable positionable)
            : this(positionable?.CurrentPosition, positionable?.Size)
        {
        }
    }
}
