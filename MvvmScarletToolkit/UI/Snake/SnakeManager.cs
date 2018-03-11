using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Timers;
using System.Windows.Input;

namespace MvvmScarletToolkit
{
    public sealed class SnakeManager : ObservableObject
    {
        private readonly SnakeOptions _options;
        private readonly ConcurrentBag<Apple> _apples;

        private bool _isLoaded = false;
        private Timer _timer;

        public int UpperBoundX { get; }
        public int LowerBoundX { get; }

        public int UpperBoundY { get; }
        public int LowerBoundY { get; }


        private Snake _snake;
        public Snake Snake
        {
            get { return _snake; }
            private set { SetValue(ref _snake, value); }
        }

        private ObservableCollection<IPositionable> _boardPieces;
        public ObservableCollection<IPositionable> BoardPieces
        {
            get { return _boardPieces; }
            private set { SetValue(ref _boardPieces, value); }
        }

        public ICommand PlayCommand { get; }
        public ICommand ResetCommand { get; }

        public ICommand MoveNorthCommand { get; }
        public ICommand MoveSouthCommand { get; }
        public ICommand MoveWestCommand { get; }
        public ICommand MoveEastCommand { get; }

        public ICommand LoadCommand { get; }

        private GameState _state;
        public GameState State
        {
            get { return _state; }
            private set { SetValue(ref _state, value); }
        }

        public SnakeManager()
            : this(new SnakeOptions())
        {
        }

        public SnakeManager(SnakeOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _apples = new ConcurrentBag<Apple>();

            LowerBoundY = 0;
            LowerBoundX = 0;

            UpperBoundY = _options.MaxHeight;
            UpperBoundX = _options.MaxWidth;

            Snake = new Snake(_options);
            BoardPieces = new ObservableCollection<IPositionable>();

            PlayCommand = new RelayCommand(Play);
            ResetCommand = new RelayCommand(Reset);

            LoadCommand = new RelayCommand(Load, CanLoad);

            MoveNorthCommand = new RelayCommand(MoveNorth, CanMoveNorth);
            MoveSouthCommand = new RelayCommand(MoveSouth, CanMoveSouth);
            MoveWestCommand = new RelayCommand(MoveWest, CanMoveWest);
            MoveEastCommand = new RelayCommand(MoveEast, CanMoveEast);
        }

        private void Play()
        {
            switch (State)
            {
                case GameState.None:
                    Start();
                    break;
                case GameState.Running:
                    Pause();
                    break;
                case GameState.Paused:
                    Resume();
                    break;
            }
        }

        private bool CanLoad()
        {
            return !_isLoaded;
        }

        private void Load()
        {
            UpdateBoardPieces();


            _isLoaded = true;
        }

        private void Reset()
        {
            State = GameState.None;
            _boardPieces.Clear();
            Start();
        }

        private void Pause()
        {
            State = GameState.Paused;
            // pause timer
        }

        private void Resume()
        {
            State = GameState.Running;
            // pause timer
        }

        private void Start()
        {
            State = GameState.Running;
            UpdateBoardPieces();
            _timer = CreateTimer();

            // start timer
            // set initial movement direction
        }

        private bool CanMoveNorth()
        {
            return State.HasFlag(GameState.Running);
        }

        private void MoveNorth()
        {
            Snake.MoveNorth();
        }

        private bool CanMoveSouth()
        {
            return State.HasFlag(GameState.Running);
        }

        private void MoveSouth()
        {
            Snake.MoveSouth();
        }

        private bool CanMoveWest()
        {
            return State.HasFlag(GameState.Running);
        }

        private void MoveWest()
        {
            Snake.MoveWest();
        }

        private bool CanMoveEast()
        {
            return State.HasFlag(GameState.Running);
        }

        private void MoveEast()
        {
            Snake.MoveEast();
        }

        private void UpdateBoardPieces()
        {
            if (!_boardPieces.Contains(Snake.Head))
                _boardPieces.Add(Snake.Head);

            foreach (var part in Snake.Body)
            {
                if (!_boardPieces.Contains(part))
                    _boardPieces.Add(part);
            }

            foreach (var apple in _apples)
            {
                if (!_boardPieces.Contains(apple))
                    _boardPieces.Add(apple);
            }
        }

        private Timer CreateTimer()
        {
            return new Timer
            {
                AutoReset = true,
                Interval = _options.GlobalTickRate,
            };
        }
    }
}
