using MvvmScarletToolkit;
using MvvmScarletToolkit.Observables;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace DemoApp
{
    public sealed class Snake : ObservableObject
    {
        private readonly SnakeOption _options;
        private readonly IMessenger _messenger;

        private SnakeHead _head;
        public SnakeHead Head
        {
            get { return _head; }
            private set { SetValue(ref _head, value); }
        }

        private ObservableCollection<SnakeSegment> _body;
        public ObservableCollection<SnakeSegment> Body
        {
            get { return _body; }
            private set { SetValue(ref _body, value); }
        }

        public Snake(SnakeOption options, IMessenger messenger)
        {
            _messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
            _options = options ?? throw new ArgumentNullException(nameof(options));

            Body = new ObservableCollection<SnakeSegment>();
            Head = new SnakeHead(options, messenger);
        }

        public bool MoveNorth()
        {
            var head = Head.CurrentPosition;
            var position = Head.MoveNorth();

            if (CanMove(position))
            {
                Head.Move(position);
            }
            else
            {
                return false;
            }

            MoveTail(head);

            return true;
        }

        public bool MoveSouth()
        {
            var head = Head.CurrentPosition;
            var position = Head.MoveSouth();

            if (CanMove(position))
            {
                Head.Move(position);
            }
            else
            {
                return false;
            }

            MoveTail(head);

            return true;
        }

        public bool MoveWest()
        {
            var head = Head.CurrentPosition;
            var position = Head.MoveWest();

            if (CanMove(position))
            {
                Head.Move(position);
            }
            else
            {
                return false;
            }

            MoveTail(head);

            return true;
        }

        public bool MoveEast()
        {
            var head = Head.CurrentPosition;
            var position = Head.MoveEast();

            if (CanMove(position))
            {
                Head.Move(position);
            }
            else
            {
                return false;
            }

            MoveTail(head);

            return true;
        }

        private bool CanMove(Position position)
        {
            if (Body.Count < 4) // snake is not long enough to bite itself
            {
                return true;
            }

            var model = new PositionDTO(Head)
            {
                CurrentPosition = position
            };

            var result = Body.OrderBy(p => p.Sequence)
                             .Skip(3)
                             .FirstOrDefault(p => model.Intersect(p));

            return result == null;
        }

        private void MoveTail(Position initialPosition)
        {
            var previousPosition = initialPosition;
            foreach (var part in _body.Where(p => !(p is null)))
            {
                var temp = part.CurrentPosition;

                if (previousPosition == temp)
                {
                    continue;
                }

                part.Move(previousPosition);
                previousPosition = temp;
            }
        }

        public bool IsEating(IPositionable boardPiece)
        {
            if (!(boardPiece is Apple))
            {
                return false;
            }

            // simple calculation if an apple intersects with the snake head, which will consume the
            // apple by default and add new Snake segment

            if (Head.Intersect(boardPiece))
            {
                var segment = Body.LastOrDefault();
                if (segment != null)
                {
                    segment = new SnakeSegment(_options, segment, _messenger, Body.Count);
                }
                else
                {
                    segment = new SnakeSegment(_options, Head, _messenger, Body.Count);
                }

                Body.Add(segment);

                _messenger.Publish(new SnakeSegmentCreatedMessage(this, segment));
                return true;
            }

            return false;
        }
    }
}
