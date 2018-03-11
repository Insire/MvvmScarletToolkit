using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace MvvmScarletToolkit
{
    public sealed class SnakeManager : ObservableObject
    {
        private readonly object _syncRoot;
        private readonly SnakeOptions _options;
        private readonly ConcurrentBag<Apple> _apples;
        private readonly Random _random;

        private bool _isLoaded = false;

        private Task _snakeTask;
        private CancellationTokenSource _snakeSource;

        private Task _appleTask;
        private CancellationTokenSource _appleSource;

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

        private Direction _direction;
        public Direction Direction
        {
            get { return _direction; }
            private set { SetValue(ref _direction, value); }
        }

        public SnakeManager()
            : this(new SnakeOptions())
        {
        }

        public SnakeManager(SnakeOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _apples = new ConcurrentBag<Apple>();
            _random = new Random();
            _syncRoot = new object();

            LowerBoundY = 0;
            LowerBoundX = 0;

            UpperBoundY = _options.MaxHeight;
            UpperBoundX = _options.MaxWidth;

            Snake = new Snake(_options);
            BoardPieces = new ObservableCollection<IPositionable>();
            BindingOperations.EnableCollectionSynchronization(BoardPieces, _syncRoot);

            PlayCommand = new RelayCommand(Play);
            ResetCommand = AsyncCommand.Create(Reset);

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

        private void Start()
        {
            State = GameState.Running;
            UpdateBoardPieces();

            _snakeSource = new CancellationTokenSource();
            _snakeTask = CreateSnakeLoop(_snakeSource.Token);

            _appleSource = new CancellationTokenSource();
            _appleTask = CreateAppleLoop(_appleSource.Token);
        }

        private async Task Reset()
        {
            State = GameState.None;

            _boardPieces.Clear();

            _snakeSource.Cancel();

            await _snakeTask;

            _snakeSource.Dispose();
            _snakeTask = null;

            _appleSource.Cancel();

            await _appleTask;

            _appleSource.Dispose();
            _appleTask = null;

            Snake = new Snake(_options);
            UpdateBoardPieces();
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

        private bool CanMoveNorth()
        {
            return State.HasFlag(GameState.Running);
        }

        private void MoveNorth()
        {
            Snake.MoveNorth();
            Direction = Direction.North;
        }

        private bool CanMoveSouth()
        {
            return State.HasFlag(GameState.Running);
        }

        private void MoveSouth()
        {
            Snake.MoveSouth();
            Direction = Direction.South;
        }

        private bool CanMoveWest()
        {
            return State.HasFlag(GameState.Running);
        }

        private void MoveWest()
        {
            Snake.MoveWest();
            Direction = Direction.West;
        }

        private bool CanMoveEast()
        {
            return State.HasFlag(GameState.Running);
        }

        private void MoveEast()
        {
            Snake.MoveEast();
            Direction = Direction.East;
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

        private Task CreateSnakeLoop(CancellationToken token)
        {
            // we create a new thread, since this is most likely running for a while and unrelated to IO stuff
            return Task.Factory.StartNew(async () =>
            {
                while (State != GameState.None)
                {
                    if (State != GameState.Paused)
                    {
                        switch (Direction)
                        {
                            case Direction.North:
                                MoveNorth();
                                break;

                            case Direction.South:
                                MoveSouth();
                                break;

                            case Direction.West:
                                MoveWest();
                                break;

                            case Direction.East:
                                MoveEast();
                                break;
                        }
                    }
                    await Task.Delay(_options.GlobalTickRate).ConfigureAwait(false);
                }

            }, token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        private Task CreateAppleLoop(CancellationToken token)
        {
            return Task.Factory.StartNew(async () =>
            {
                while (State != GameState.None)
                {
                    if (Direction != Direction.None && _options.MaxFoodCount > _apples.Count) // start generating appls, as soon as the snake moves
                        _apples.Add(new Apple(_random.Next(LowerBoundX, UpperBoundX), _random.Next(UpperBoundY, UpperBoundY)));

                    DispatcherFactory.Invoke(() =>
                    {
                        UpdateBoardPieces();
                    }, DispatcherPriority.Send);
                    await Task.Delay(_options.FoodTickRate).ConfigureAwait(false);
                }

            }, token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }
    }
}
