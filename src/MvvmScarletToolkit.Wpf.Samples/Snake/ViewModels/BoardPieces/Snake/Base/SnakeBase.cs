using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Messaging;
using System;

namespace MvvmScarletToolkit.Wpf.Samples
{
    public abstract class SnakeBase : ObservableRecipient, IPositionable
    {
        private readonly SnakeOption _options;

        private Position _currentPosition;
        public Position CurrentPosition
        {
            get { return _currentPosition; }
            protected set { SetProperty(ref _currentPosition, value); }
        }

        private Size _size;
        public Size Size
        {
            get { return _size; }
            protected set { SetProperty(ref _size, value); }
        }

        protected SnakeBase(SnakeOption options, Size size, IMessenger messenger)
            : base(messenger)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            Size = size ?? throw new ArgumentNullException(nameof(size));
        }

        protected SnakeBase(SnakeOption options, IMessenger messenger)
            : this(options, new Size(options.BoardPieceSize, options.BoardPieceSize), messenger)
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

            Messenger.Send(message);
        }
    }
}
