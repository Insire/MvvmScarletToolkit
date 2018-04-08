using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace MvvmScarletToolkit
{
    public sealed class Snake : ObservableObject
    {
        private readonly SnakeOption _options;
        private readonly ILogger _log;

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

        public Snake(SnakeOption options, ILogger log)
        {
            _log = log ?? throw new ArgumentNullException(nameof(log));
            _options = options ?? throw new ArgumentNullException(nameof(options));

            Body = new ObservableCollection<SnakeSegment>();
            Head = new SnakeHead(options, log);
        }

        public bool MoveNorth()
        {
            var head = Head.CurrentPosition;
            var position = Head.MoveNorth();

            if (CanMove(position))
                Head.Move(position);
            else
                return false;

            MoveTail(head);

            return true;
        }

        public bool MoveSouth()
        {
            var head = Head.CurrentPosition;
            var position = Head.MoveSouth();

            if (CanMove(position))
                Head.Move(position);
            else
                return false;

            MoveTail(head);

            return true;
        }

        public bool MoveWest()
        {
            var head = Head.CurrentPosition;
            var position = Head.MoveWest();

            if (CanMove(position))
                Head.Move(position);
            else
                return false;

            MoveTail(head);

            return true;
        }

        public bool MoveEast()
        {
            var head = Head.CurrentPosition;
            var position = Head.MoveEast();

            if (CanMove(position))
                Head.Move(position);
            else
                return false;

            MoveTail(head);

            return true;
        }

        private bool CanMove(Position position)
        {
            if (Body.Count < 4) // snake is not long enough to bite itself
                return true;

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
            foreach (var part in _body.Where(p => p is SnakeSegment))
            {
                var temp = part.CurrentPosition;

                if (previousPosition == temp)
                    continue;

                part.Move(previousPosition);
                previousPosition = temp;
            }
        }

        public bool IsEating(IPositionable boardPiece)
        {
            if (!(boardPiece is Apple))
                return false;

            // simple calculation if an apple intersects with the snake head, which will consume the
            // apple by default and add new Snake segment

            if (Head.Intersect(boardPiece))
            {
                _log.Log($"EAT: {_head.CurrentPosition.X};{_head.CurrentPosition.Y}");

                var segment = Body.LastOrDefault();
                if (segment != null)
                    segment = new SnakeSegment(_options, segment, _log, Body.Count);
                else
                    segment = new SnakeSegment(_options, Head, _log, Body.Count);

                Body.Add(segment);

                return true;
            }

            return false;
        }
    }
}
