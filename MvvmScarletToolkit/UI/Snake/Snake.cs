using System;
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
            Head.MoveNorth();

            if (_isFed)
            {
                _isFed = false;
                return;
            }

            foreach (var part in _body.Where(p => p is SnakeSegment))
                part.MoveNorth();
        }

        public void MoveSouth()
        {
            Head.MoveSouth();

            if (_isFed)
            {
                _isFed = false;
                return;
            }

            foreach (var part in _body.Where(p => p is SnakeSegment))
                part.MoveSouth();
        }

        public void MoveWest()
        {
            Head.MoveWest();

            if (_isFed)
            {
                _isFed = false;
                return;
            }

            foreach (var part in _body.Where(p => p is SnakeSegment))
                part.MoveWest();
        }

        public void MoveEast()
        {
            Head.MoveEast();

            if (_isFed)
            {
                _isFed = false;
                return;
            }

            foreach (var part in _body.Where(p => p is SnakeSegment))
                part.MoveEast();
        }

        public void AddSegment()
        {
            var segment = new SnakeSegment(_options, _head);
            Body.Add(segment);

            _isFed = true;
        }
    }
}
