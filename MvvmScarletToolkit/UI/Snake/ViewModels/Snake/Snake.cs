﻿using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace MvvmScarletToolkit
{
    public sealed class Snake : ObservableObject
    {
        private readonly SnakeOptions _options;

        private bool _isFed;

        private SnakeHead _head;
        public SnakeHead Head
        {
            get { return _head; }
            private set { SetValue(ref _head, value); }
        }

        private ObservableCollection<SnakeBase> _body;
        public ObservableCollection<SnakeBase> Body
        {
            get { return _body; }
            private set { SetValue(ref _body, value); }
        }

        public Snake(SnakeOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));

            Body = new ObservableCollection<SnakeBase>();
            Head = new SnakeHead(options);
        }

        public void MoveNorth()
        {
            var head = Head.CurrentPosition;
            Head.MoveNorth();

            MoveTail(head);
        }

        public void MoveSouth()
        {
            var head = Head.CurrentPosition;
            Head.MoveSouth();

            MoveTail(head);
        }

        public void MoveWest()
        {
            var head = Head.CurrentPosition;
            Head.MoveWest();

            MoveTail(head);
        }

        public void MoveEast()
        {
            var head = Head.CurrentPosition;
            Head.MoveEast();

            MoveTail(head);
        }

        private void MoveTail(Position initialPosition)
        {
            if (_isFed)
            {
                _isFed = false;
                return;
            }

            var previousPosition = initialPosition;
            foreach (var part in _body.Where(p => p is SnakeSegment))
            {
                var temp = part.CurrentPosition;
                part.Move(previousPosition);
                previousPosition = temp;
            }
        }

        public bool IsEating(IPositionable boardPiece)
        {
            if (!(boardPiece is Apple))
                return false;

            // simple calculation if an apple intersects with the snake head, which will consume the apple by default
            // and a new Snake segment

            // XH1 < XH2
            // YH1 < YH2
            var XH1 = Head.CurrentPosition.X - Head.Size.Width;
            var YH1 = Head.CurrentPosition.Y - Head.Size.Height;
            var XH2 = Head.CurrentPosition.X + Head.Size.Width;
            var YH2 = Head.CurrentPosition.Y + Head.Size.Height;

            // XA1 < XA2
            // YA1 < YA2
            var XA1 = boardPiece.CurrentPosition.X - boardPiece.Size.Width;
            var YA1 = boardPiece.CurrentPosition.Y - boardPiece.Size.Height;
            var XA2 = boardPiece.CurrentPosition.X + boardPiece.Size.Width;
            var YA2 = boardPiece.CurrentPosition.Y + boardPiece.Size.Height;

            if ((XA2 >= XH1 && XA1 <= XH2) && (YA2 >= YH1 && YA1 <= YH2))
            {
                var segment = new SnakeSegment(_options, _head);
                Body.Add(segment);

                _isFed = true;

                return true;
            }

            return false;
        }
    }
}
