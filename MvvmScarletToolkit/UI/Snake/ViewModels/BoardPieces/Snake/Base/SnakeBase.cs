
using System;

namespace MvvmScarletToolkit
{
    public abstract class SnakeBase : ObservableObject, IPositionable
    {
        private readonly ILogger _log;
        private readonly SnakeOptions _options;
        private readonly string _debugName;

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

        public SnakeBase(SnakeOptions options, Size size, ILogger log)
        {
            _log = log ?? throw new ArgumentNullException(nameof(log));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            Size = size ?? throw new ArgumentNullException(nameof(size));

            _debugName = $"DEBUG: {GetType().Name}".PadRight(35);
        }

        public SnakeBase(SnakeOptions options, ILogger log)
            : this(options, new Size(options.BoardPieceSize, options.BoardPieceSize), log)
        {
        }

        /// <summary>
        /// move relative
        /// </summary>
        public Position MoveNorth()
        {
            return new Position()
            {
                X = _currentPosition.X,
                Y = _currentPosition.Y - _options.StepWidth,
            };
        }

        /// <summary>
        /// move relative
        /// </summary>
        public Position MoveSouth()
        {
            return new Position()
            {
                X = _currentPosition.X,
                Y = _currentPosition.Y + _options.StepWidth,
            };
        }

        /// <summary>
        /// move relative
        /// </summary>
        public Position MoveWest()
        {
            return new Position()
            {
                X = _currentPosition.X - _options.StepWidth,
                Y = _currentPosition.Y,
            };
        }

        /// <summary>
        /// move relative
        /// </summary>
        public Position MoveEast()
        {
            return new Position()
            {
                X = _currentPosition.X + _options.StepWidth,
                Y = _currentPosition.Y,
            };
        }

        /// <summary>
        /// move to absolute position
        /// </summary>
        internal void Move(Position position)
        {
            if (position.X > _options.MaxWidth)
                position.X = 0;
            if (position.X < 0)
                position.X = _options.MaxWidth;

            if (position.Y > _options.MaxHeight)
                position.Y = 0;
            if (position.Y < 0)
                position.Y = _options.MaxHeight;

            _log.Log($"{_debugName} {CurrentPosition.X};{CurrentPosition.Y} -> {position.X};{position.Y}");

            CurrentPosition = position;
        }
    }
}
