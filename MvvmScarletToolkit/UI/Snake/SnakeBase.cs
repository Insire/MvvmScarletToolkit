using System;

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

        public SnakeBase(SnakeOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
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
