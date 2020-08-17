using MvvmScarletToolkit.Observables;
using System;

namespace MvvmScarletToolkit.Samples
{
    public sealed class SnakeOption : ObservableObject
    {
        public static SnakeOption Easy()
        {
            return new SnakeOption()
            {
                SpeedMultiplier = 0.1,
                IsDefault = true,
                Name = "Easy",
            };
        }

        public static SnakeOption Normal()
        {
            return new SnakeOption()
            {
                SpeedMultiplier = 0.075,
                IsDefault = true,
                Name = "Normal",
            };
        }

        public static SnakeOption Hard()
        {
            return new SnakeOption()
            {
                SpeedMultiplier = 0.035,
                IsDefault = true,
                Name = "Hard",
            };
        }

        private bool _isDefault;
        /// <summary>
        /// indicates whether this instance should be modified via the UI
        /// </summary>
        public bool IsDefault
        {
            get { return _isDefault; }
            private set { SetValue(ref _isDefault, value); }
        }

        private string _name;
        public string Name
        {
            get { return _name; }
            private set { SetValue(ref _name, value); }
        }

        private int _fieldSize;
        /// <summary>
        /// Size of a BoardTile
        /// </summary>
        public int FieldSize
        {
            get { return _fieldSize; }
            set
            {
                if (SetValue(ref _fieldSize, value))
                {
                    OnPropertyChanged(nameof(StepWidth));
                    OnPropertyChanged(nameof(MaxWidth));
                    OnPropertyChanged(nameof(MaxHeight));
                    OnPropertyChanged(nameof(BoardPieceSize));
                }
            }
        }

        private int _fieldCountX;
        /// <summary>
        /// max amount of fields on x-axis
        /// </summary>
        public int FieldCountX
        {
            get { return _fieldCountX; }
            set
            {
                if (SetValue(ref _fieldCountX, value))
                {
                    OnPropertyChanged(nameof(MaxWidth));
                }
            }
        }

        private int _fieldCountY;
        /// <summary>
        /// max amount of fields on y-axis
        /// </summary>
        public int FieldCountY
        {
            get { return _fieldCountY; }
            set
            {
                if (SetValue(ref _fieldCountY, value))
                {
                    OnPropertyChanged(nameof(MaxHeight));
                }
            }
        }

        private double _speedMultiplier;

        /// <summary>
        /// a value between 0 and 1
        /// </summary>
        public double SpeedMultiplier
        {
            get { return _speedMultiplier; }
            set { SetValue(ref _speedMultiplier, value); }
        }

        private int _foodInterval;

        /// <summary>
        /// How often food should be generated
        /// </summary>
        public int FoodInterval
        {
            get { return _foodInterval; }
            set
            {
                if (SetValue(ref _foodInterval, value))
                {
                    OnPropertyChanged(nameof(FoodTickRate));
                }
            }
        }

        private int _maxFoodCount;
        /// <summary>
        /// maximum amount of food units
        /// </summary>
        public int MaxFoodCount
        {
            get { return _maxFoodCount; }
            set { SetValue(ref _maxFoodCount, value); }
        }

        /// <summary>
        /// determines how many units a board piece can move per tick
        /// </summary>
        public int StepWidth => FieldSize;

        public int BoardPieceSize => FieldSize - 1;

        /// <summary>
        /// amount of horizontal units
        /// </summary>
        public int MaxWidth => FieldCountX * FieldSize;

        /// <summary>
        /// amount of vertical units
        /// </summary>
        public int MaxHeight => FieldCountY * FieldSize;

        public int MinWidth => 0;

        public int MinHeight => 0;

        /// <summary>
        /// interval of which the snake gets updated
        /// </summary>
        public int GlobalTickRate => Convert.ToInt32(Math.Round(100 * SpeedMultiplier, MidpointRounding.AwayFromZero));

        /// <summary>
        /// interval in milliseconds of which apples can be generated
        /// </summary>
        public int FoodTickRate => Convert.ToInt32(Math.Round(1000 * SpeedMultiplier * FoodInterval, MidpointRounding.AwayFromZero));

        public SnakeOption()
        {
            FieldSize = 16;

            FieldCountX = 100;
            FieldCountY = 100;

            SpeedMultiplier = 0.075;
            FoodInterval = 3;
            MaxFoodCount = 5;

            IsDefault = false;
            Name = "Custom";
        }

        public Position GetStartingPosition()
        {
            return new Position()
            {
                X = Convert.ToInt32(Math.Round((double)MaxWidth / 2, MidpointRounding.AwayFromZero)),
                Y = Convert.ToInt32(Math.Round((double)MaxHeight / 2, MidpointRounding.AwayFromZero)),
            };
        }
    }
}
