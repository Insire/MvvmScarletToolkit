using System;

using MvvmScarletToolkit.SnakeGame;

namespace MvvmScarletToolkit
{
    public abstract class SnakeBase : ObservableObject, IPositionable
    {
        private readonly SnakeOptions _options;

        private Position _currentPosition;
        public Position CurrentPosition
        {
            get { return _currentPosition; }
            protected set { SetValue(ref _currentPosition, value); }
        }

        private Size _size;
        public Size Size
        {
            get { return _size; }
            protected set { SetValue(ref _size, value); }
        }

        public SnakeBase(SnakeOptions options, Size size)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            Size = size ?? throw new ArgumentNullException(nameof(size));
        }

        public SnakeBase(SnakeOptions options)
            : this(options, new Size(options.FieldSize, options.FieldSize))
        {
        }

        /// <summary>
        /// move relative
        /// </summary>
        public void MoveNorth()
        {
            Move(new Position()
            {
                X = _currentPosition.X,
                Y = _currentPosition.Y - _options.StepWidth,
            });
        }

        /// <summary>
        /// move relative
        /// </summary>
        public void MoveSouth()
        {
            Move(new Position()
            {
                X = _currentPosition.X,
                Y = _currentPosition.Y + _options.StepWidth,
            });
        }

        /// <summary>
        /// move relative
        /// </summary>
        public void MoveWest()
        {
            Move(new Position()
            {
                X = _currentPosition.X - _options.StepWidth,
                Y = _currentPosition.Y,
            });
        }

        /// <summary>
        /// move relative
        /// </summary>
        public void MoveEast()
        {
            Move(new Position()
            {
                X = _currentPosition.X + _options.StepWidth,
                Y = _currentPosition.Y,
            });
        }

        /// <summary>
        /// move to absolute position
        /// </summary>
        private void Move(Position position)
        {
            CurrentPosition = position;
        }
    }
}
