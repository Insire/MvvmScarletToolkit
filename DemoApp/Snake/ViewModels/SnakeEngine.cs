using MvvmScarletToolkit.Commands;
using MvvmScarletToolkit.Observables;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace MvvmScarletToolkit
{
    internal sealed class SnakeEngine : ObservableObject, ISnakeManager
    {
        private readonly Dispatcher _dispatcher;
        private readonly SnakeOption _options;
        private readonly IProducerConsumerCollection<Apple> _apples;
        private readonly Random _random;
        private readonly IMessenger _messenger;

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

        private IAsyncCommand _playCommand;
        public IAsyncCommand PlayCommand
        {
            get { return _playCommand; }
            private set { SetValue(ref _playCommand, value); }
        }

        private IAsyncCommand _resetCommand;
        public IAsyncCommand ResetCommand
        {
            get { return _resetCommand; }
            private set { SetValue(ref _resetCommand, value); }
        }

        private ICommand _moveNorthCommand;
        public ICommand MoveNorthCommand
        {
            get { return _moveNorthCommand; }
            private set { SetValue(ref _moveNorthCommand, value); }
        }

        private ICommand _moveSouthCommand;
        public ICommand MoveSouthCommand
        {
            get { return _moveSouthCommand; }
            private set { SetValue(ref _moveSouthCommand, value); }
        }

        private ICommand _moveWestCommand;
        public ICommand MoveWestCommand
        {
            get { return _moveWestCommand; }
            private set { SetValue(ref _moveWestCommand, value); }
        }

        private ICommand _moveEastCommand;
        public ICommand MoveEastCommand
        {
            get { return _moveEastCommand; }
            private set { SetValue(ref _moveEastCommand, value); }
        }

        private ICommand _loadCommand;
        public ICommand LoadCommand
        {
            get { return _loadCommand; }
            private set { SetValue(ref _loadCommand, value); }
        }

        private volatile GameState _state;
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

        private volatile Direction _direction;
        public Direction Direction
        {
            get { return _direction; }
            private set
            {
                if (_direction == value)
                    return;

                _direction = value;
                OnPropertyChanged(nameof(Direction));
                _messenger.Publish(new SnakeDirectionChanged(this, value));
            }
        }

        public SnakeEngine(SnakeOption options, Dispatcher dispatcher, IMessenger messenger)
        {
            _messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _apples = new ConcurrentQueue<Apple>();
            _random = new Random();

            _direction = Direction.North;
            _state = GameState.None;

            LowerBoundY = _options.MinHeight;
            LowerBoundX = _options.MinWidth;

            UpperBoundY = _options.MaxHeight;
            UpperBoundX = _options.MaxWidth;

            Snake = new Snake(_options, _messenger);
            BoardPieces = new ObservableCollection<IPositionable>();

            PlayCommand = AsyncCommand.Create(Play);
            ResetCommand = AsyncCommand.Create(Reset);

            LoadCommand = new RelayCommand(Load, CanLoad);

            MoveNorthCommand = new RelayCommand(SetDirectionNorth, CanMoveNorth);
            MoveSouthCommand = new RelayCommand(SetDirectionSouth, CanMoveSouth);
            MoveWestCommand = new RelayCommand(SetDirectionWest, CanMoveWest);
            MoveEastCommand = new RelayCommand(SetDirectionEast, CanMoveEast);
        }

        private async Task Play()
        {
            switch (State)
            {
                case GameState.None:
                    await Start();
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

        private Task Start()
        {
            State = GameState.Running;
            UpdateBoardPieces();

            _snakeSource = new CancellationTokenSource();
            _snakeTask = CreateSnakeLoop(_snakeSource.Token);

            _appleSource = new CancellationTokenSource();
            _appleTask = CreateAppleLoop(_appleSource.Token);

            return _snakeTask;
        }

        private async Task Reset()
        {
            State = GameState.None;

            if (_snakeSource != null)
            {
                _snakeSource.Cancel();
                _snakeSource.Dispose();
                _snakeSource = null;
            }

            if (_snakeTask != null)
            {
                await _snakeTask.ConfigureAwait(true);
                _snakeTask = null;
            }

            if (_appleSource != null)
            {
                _appleSource.Cancel();
                _appleSource.Dispose();
                _appleSource = null;
            }

            if (_appleTask != null)
            {
                await _appleTask.ConfigureAwait(true);
                _appleTask = null;
            }

            _boardPieces.Clear();
            _apples.Clear();

            Snake = new Snake(_options, _messenger);
            UpdateBoardPieces();
        }

        private void Pause()
        {
            State = GameState.Paused;
        }

        private void Resume()
        {
            State = GameState.Running;
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
            var error = true;
            try
            {
                await _dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action<Direction>((d) =>
                {
                    if (!InternalMove(d))
                        State = GameState.Fail;

                    error = false;
                }), _direction);
            }
            catch (TaskCanceledException)
            {
                // game loop was cancellled, nothing to see here...move on
            }
            finally
            {
                if (error)
                    State = GameState.Fail;
            }
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

            if (BoardPieces.Count(p => p is Apple) < _options.MaxFoodCount && _apples.TryTake(out var apple))
                BoardPieces.Add(apple);
        }

        private Task CreateSnakeLoop(CancellationToken token)
        {
            // we create a new thread, since this is most likely running for a while and unrelated to
            // IO stuff
            return Task.Factory.StartNew(async () =>
            {
                try
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
                }
                catch (TaskCanceledException)
                {
                    // game loop was cancellled, nothing to see here...move on
                }
            }, token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        private Task CreateAppleLoop(CancellationToken token)
        {
            return Task.Factory.StartNew(async () =>
            {
                try
                {
                    while (State != GameState.None)
                    {
                        if (State == GameState.Fail)
                            return;

                        if (Direction != Direction.None && _options.MaxFoodCount > _apples.Count)
                            _apples.TryAdd(CreateApple(_options, _random, Snake));

                        if (_appleSource?.Token == null || _appleSource.Token.IsCancellationRequested)
                            return;

                        await Task.Delay(_options.FoodTickRate);
                    }
                }
                catch (TaskCanceledException)
                {
                    // game loop was cancellled, nothing to see here...move on
                }
            }, token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        private static Apple CreateApple(SnakeOption option, Random random, Snake snake)
        {
            var result = new PositionDTO(snake.Head);
            var snakeParts = default(List<SnakeBase>);

            do
            {
                snakeParts = new List<SnakeBase>(snake.Body)
                {
                    snake.Head
                };

                result.CurrentPosition = new Position(random.Next(option.MinWidth, option.MaxWidth), random.Next(option.MinHeight, option.MaxHeight));
            }
            while (snakeParts.Any(p => result.Intersect(p)));

            return new Apple(result);
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
            if (!_disposed)
            {
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

                _disposed = true;
            }
        }
    }
}
