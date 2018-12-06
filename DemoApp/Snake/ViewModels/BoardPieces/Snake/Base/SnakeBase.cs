using MvvmScarletToolkit;
using MvvmScarletToolkit.Observables;
using System;

namespace DemoApp
{
    public abstract class SnakeBase : ObservableObject, IPositionable
    {
        private readonly IMessenger _messenger;
        private readonly SnakeOption _options;

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

        protected SnakeBase(SnakeOption options, Size size, IMessenger messenger)
        {
            _messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            Size = size ?? throw new ArgumentNullException(nameof(size));
        }

        protected SnakeBase(SnakeOption options, IMessenger log)
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
        internal void Move(Position newPosition)
        {
            if (newPosition.X > _options.MaxWidth)
            {
                newPosition.X = 0;
            }

            if (newPosition.X < 0)
            {
                newPosition.X = _options.MaxWidth;
            }

            if (newPosition.Y > _options.MaxHeight)
            {
                newPosition.Y = 0;
            }

            if (newPosition.Y < 0)
            {
                newPosition.Y = _options.MaxHeight;
            }

            var message = new PositionUpdatedMessage(this, CurrentPosition, newPosition);

            CurrentPosition = newPosition;

            _messenger.Publish(message);
        }
    }
}
