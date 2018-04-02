using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace MvvmScarletToolkit
{
    public sealed class SnakeManager : ObservableObject, IRefresh
    {
        private readonly Dispatcher _dispatcher;
        private readonly SnakeOptions _options;
        private readonly IProducerConsumerCollection<Apple> _apples;
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

        public IAsyncCommand MoveNorthCommand { get; }
        public IAsyncCommand MoveSouthCommand { get; }
        public IAsyncCommand MoveWestCommand { get; }
        public IAsyncCommand MoveEastCommand { get; }

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

        public SnakeManager(SnakeOptions options, Dispatcher dispatcher)
        {
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _apples = new ConcurrentQueue<Apple>();
            _random = new Random();

            LowerBoundY = 0;
            LowerBoundX = 0;

            UpperBoundY = _options.MaxHeight;
            UpperBoundX = _options.MaxWidth;

            Snake = new Snake(_options);
            BoardPieces = new ObservableCollection<IPositionable>();

            PlayCommand = new RelayCommand(Play);
            ResetCommand = AsyncCommand.Create(Reset);

            LoadCommand = new RelayCommand(Load, CanLoad);

            MoveNorthCommand = AsyncCommand.Create(MoveNorth, CanMoveNorth);
            MoveSouthCommand = AsyncCommand.Create(MoveSouth, CanMoveSouth);
            MoveWestCommand = AsyncCommand.Create(MoveWest, CanMoveWest);
            MoveEastCommand = AsyncCommand.Create(MoveEast, CanMoveEast);
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

        private Task MoveNorth()
        {
            return InternalMoveAsync(Direction.North);
        }

        private bool CanMoveSouth()
        {
            return State.HasFlag(GameState.Running);
        }

        private Task MoveSouth()
        {
            return InternalMoveAsync(Direction.South);
        }

        private bool CanMoveWest()
        {
            return State.HasFlag(GameState.Running);
        }

        private Task MoveWest()
        {
            return InternalMoveAsync(Direction.West);
        }

        private bool CanMoveEast()
        {
            return State.HasFlag(GameState.Running);
        }

        private Task MoveEast()
        {
            return InternalMoveAsync(Direction.East);
        }

        private async Task InternalMoveAsync(Direction direction)
        {
            await _dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action<Direction>((d) => InternalMove(d)), direction);
        }

        private void InternalMove(Direction direction)
        {
            switch (direction)
            {
                case Direction.East:
                    Snake.MoveEast();
                    break;
                case Direction.West:
                    Snake.MoveWest();
                    break;
                case Direction.North:
                    Snake.MoveNorth();
                    break;
                case Direction.South:
                    Snake.MoveSouth();
                    break;
                default:
                    return;
            }

            Direction = direction;

            var tastyApple = IsSnakeEatingApple();
            if (tastyApple == null)
                return;

            BoardPieces.Remove(tastyApple);
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
                                await MoveNorth().ConfigureAwait(false);
                                break;

                            case Direction.South:
                                await MoveSouth().ConfigureAwait(false);
                                break;

                            case Direction.West:
                                await MoveWest().ConfigureAwait(false);
                                break;

                            case Direction.East:
                                await MoveEast().ConfigureAwait(false);
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
                    if (Direction != Direction.None && _options.MaxFoodCount > _apples.Count) // start generating apples, as soon as the snake moves
                        _apples.TryAdd(new Apple(_random.Next(LowerBoundX, UpperBoundX), _random.Next(LowerBoundY, UpperBoundY), _options));

                    await Task.Delay(1000);
                }

            }, token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        public void Refresh()
        {
            if (State == GameState.Running)
                UpdateBoardPieces();
        }
    }
}
