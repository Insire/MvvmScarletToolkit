using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace MvvmScarletToolkit
{
    public sealed class SnakeManager : ObservableObject, ISnakeManager
    {
        private readonly Dispatcher _dispatcher;
        private readonly SnakeOptions _options;
        private readonly IProducerConsumerCollection<Apple> _apples;
        private readonly Random _random;
        private readonly ILogger _log;

        private bool _isLoaded = false;
        private bool _disposed = false;

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
            private set
            {
                if (_state == value)
                    return;

                _state = value;
                OnPropertyChanged(nameof(State));
            }
        }

        private Direction _direction;
        public Direction Direction
        {
            get { return _direction; }
            private set
            {
                if (_direction == value)
                    return;

                _direction = value;
                OnPropertyChanged(nameof(Direction));
            }
        }

        public SnakeManager(SnakeOptions options, Dispatcher dispatcher, ILogger log)
        {
            _log = log ?? throw new ArgumentNullException(nameof(log));
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _apples = new ConcurrentQueue<Apple>();
            _random = new Random();

            _direction = Direction.North;
            _state = GameState.None;

            LowerBoundY = 0;
            LowerBoundX = 0;

            UpperBoundY = _options.MaxHeight;
            UpperBoundX = _options.MaxWidth;

            Snake = new Snake(_options, _log);
            BoardPieces = new ObservableCollection<IPositionable>();

            PlayCommand = new RelayCommand(Play);
            ResetCommand = AsyncCommand.Create(Reset);

            LoadCommand = new RelayCommand(Load, CanLoad);

            MoveNorthCommand = new RelayCommand(SetDirectionNorth, CanMoveNorth);
            MoveSouthCommand = new RelayCommand(SetDirectionSouth, CanMoveSouth);
            MoveWestCommand = new RelayCommand(SetDirectionWest, CanMoveWest);
            MoveEastCommand = new RelayCommand(SetDirectionEast, CanMoveEast);
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

            if (_snakeTask != null)
                await _snakeTask;

            if (_snakeSource != null)
            {
                _snakeSource.Cancel();
                _snakeSource.Dispose();
                _snakeSource = null;
            }
            _snakeTask = null;

            if (_appleTask != null)
                await _appleTask;

            if (_appleSource != null)
            {
                _appleSource.Cancel();
                _appleSource.Dispose();
                _appleSource = null;
            }

            _appleTask = null;

            Snake = new Snake(_options, _log);
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

        private void SetDirectionNorth()
        {
            Direction = Direction.North;
        }

        private void SetDirectionSouth()
        {
            Direction = Direction.South;
        }

        private void SetDirectionEast()
        {
            Direction = Direction.East;
        }

        private void SetDirectionWest()
        {
            Direction = Direction.West;
        }

        private bool CanMoveNorth()
        {
            return State.HasFlag(GameState.Running)
                && Direction != Direction.South
                && Direction != Direction.North;
        }

        private bool CanMoveSouth()
        {
            return State.HasFlag(GameState.Running)
                && Direction != Direction.North
                && Direction != Direction.South;
        }

        private bool CanMoveWest()
        {
            return State.HasFlag(GameState.Running)
                && Direction != Direction.East
                && Direction != Direction.West;
        }

        private bool CanMoveEast()
        {
            return State.HasFlag(GameState.Running)
                && Direction != Direction.West
                && Direction != Direction.East;
        }

        private async Task InternalMoveAsync()
        {
            await _dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action<Direction>((d) =>
            {
                if (!InternalMove(d))
                    State = GameState.Fail;
            }), _direction);
        }

        private bool InternalMove(Direction direction)
        {
            var result = false;
            switch (direction)
            {
                case Direction.East:
                    result = Snake.MoveEast();
                    break;

                case Direction.West:
                    result = Snake.MoveWest();
                    break;

                case Direction.North:
                    result = Snake.MoveNorth();
                    break;

                case Direction.South:
                    result = Snake.MoveSouth();
                    break;

                default:
                    return false;
            }

            var tastyApple = IsSnakeEatingApple();
            if (tastyApple == null)
                return result;

            BoardPieces.Remove(tastyApple);

            return result;
        }

        private IPositionable IsSnakeEatingApple()
        {
            return BoardPieces.FirstOrDefault(p => Snake.IsEating(p));
        }

        private void UpdateBoardPieces()
        {
            if (!_boardPieces.Contains(Snake.Head))
                BoardPieces.Add(Snake.Head);

            foreach (var part in Snake.Body)
            {
                if (!_boardPieces.Contains(part))
                    BoardPieces.Add(part);
            }

            if (BoardPieces.Where(p => p is Apple).Count() < _options.MaxFoodCount && _apples.TryTake(out var apple))
                BoardPieces.Add(apple);
        }

        private Task CreateSnakeLoop(CancellationToken token)
        {
            // we create a new thread, since this is most likely running for a while and unrelated to
            // IO stuff
            return Task.Factory.StartNew(async () =>
            {
                while (State != GameState.None)
                {
                    if (State == GameState.Fail)
                        return;

                    if (State != GameState.Paused)
                        await InternalMoveAsync().ConfigureAwait(false);

                    if (_snakeSource?.Token == null || _snakeSource.Token.IsCancellationRequested)
                        return;

                    await Task.Delay(_options.GlobalTickRate, _snakeSource.Token).ConfigureAwait(false);
                }
            }, token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        private Task CreateAppleLoop(CancellationToken token)
        {
            return Task.Factory.StartNew(async () =>
            {
                while (State != GameState.None)
                {
                    if (State == GameState.Fail)
                        return;

                    if (Debugger.IsAttached)
                        _apples.TryAdd(new Apple(Snake.Head.CurrentPosition.X, _random.Next(LowerBoundY, UpperBoundY), _options));
                    else
                    {
                        if (Direction != Direction.None && _options.MaxFoodCount > _apples.Count) // start generating apples, as soon as the snake moves
                            _apples.TryAdd(new Apple(_random.Next(LowerBoundX, UpperBoundX), _random.Next(LowerBoundY, UpperBoundY), _options));
                    }

                    if (_appleSource?.Token == null || _appleSource.Token.IsCancellationRequested)
                        return;

                    await Task.Delay(_options.FoodTickRate);
                }
            }, token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        public void Refresh()
        {
            if (State == GameState.Running)
                UpdateBoardPieces();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!_disposed)
            {
                // If disposing equals true, dispose all managed and unmanaged resources.
                if (disposing)
                {
                    if (_appleSource != null)
                    {
                        _appleSource.Cancel();
                        _appleSource.Dispose();
                        _appleSource = null;
                    }

                    _snakeTask?.Wait();

                    if (_snakeSource != null)
                    {
                        _snakeSource.Cancel();
                        _snakeSource.Dispose();
                        _snakeSource = null;
                    }

                    _appleTask?.Wait();

                    Reset().Wait();
                }

                // Note disposing has been done.
                _disposed = true;
            }
        }
    }
}
